using AutoMapper;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MiraiNotes.Shared.Extensions;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels
{
    public class MenuViewModel : BaseViewModel<MenuViewModelParameter>
    {
        #region Members
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IBackgroundTaskManagerService _backgroundTaskManager;
        private string _currentUserName;
        private string _currentUserEmail;
        private MvxObservableCollection<TaskListItemViewModel> _taskLists = new MvxObservableCollection<TaskListItemViewModel>();
        private readonly MvxInteraction<string> _onUserProfileImgLoaded = new MvxInteraction<string>();
        private readonly MvxInteraction<bool> _onTaskListsLoaded = new MvxInteraction<bool>();
        private readonly MvxInteraction<int> _refreshNumberOfTasks = new MvxInteraction<int>();
        #endregion

        #region Interactors
        // need to expose it as a public property for binding (only IMvxInteraction is needed in the view)
        public IMvxInteraction<string> OnUserProfileImgLoaded
            => _onUserProfileImgLoaded;
        public IMvxInteraction<bool> OnTaskListsLoaded
            => _onTaskListsLoaded;
        public IMvxInteraction<int> RefreshNumberOfTasks
            => _refreshNumberOfTasks;
        #endregion

        #region Properties
        public string CurrentUserName
        {
            get => _currentUserName;
            set => SetProperty(ref _currentUserName, value);
        }

        public string CurrentUserEmail
        {
            get => _currentUserEmail;
            set => SetProperty(ref _currentUserEmail, value);
        }

        public MvxObservableCollection<TaskListItemViewModel> TaskLists
        {
            get => _taskLists;
            set => SetProperty(ref _taskLists, value);
        }

        public TaskListItemViewModel SelectedTaskList { get; private set; }

        public NotificationAction InitParams { get; set; }
        #endregion

        #region Commands
        public IMvxAsyncCommand<int> OnTaskListSelectedCommand { get; private set; }
        #endregion

        public MenuViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IMapper mapper,
            IDialogService dialogService,
            IMiraiNotesDataService dataService,
            IAppSettingsService appSettings,
            IBackgroundTaskManagerService backgroundTaskManager,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<MenuViewModel>(), navigationService, appSettings, telemetryService)
        {
            _mapper = mapper;
            _dialogService = dialogService;
            _dataService = dataService;
            _backgroundTaskManager = backgroundTaskManager;
        }

        public override void Prepare(MenuViewModelParameter parameter)
        {
            base.Prepare(parameter);
            InitParams = parameter.Notification;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            //TODO: AVOID RELOADING EVERYTHING ON SCREEN CHANGES
            Messenger.Publish(new ShowTasksLoadingMsg(this));
            Messenger.Publish(new ShowProgressOverlayMsg(this));
            await LoadProfileInfo();

            await InitView();
            Messenger.Publish(new ShowTasksLoadingMsg(this, false));
            Messenger.Publish(new ShowProgressOverlayMsg(this, false));
        }

        public override void SetCommands()
        {
            base.SetCommands();
            OnTaskListSelectedCommand = new MvxAsyncCommand<int>(OnTaskListSelected);
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
            var tokens = new[]
            {
                Messenger.Subscribe<ActiveAccountChangedMsg>(async(msg) =>
                {
                    Messenger.Publish(new ShowTasksLoadingMsg(this));
                    Messenger.Publish(new ShowProgressOverlayMsg(this));

                    await LoadProfileInfo();

                    await InitView(true);

                    Messenger.Publish(new ShowTasksLoadingMsg(this, false));
                    Messenger.Publish(new ShowProgressOverlayMsg(this, false));
                }),
                Messenger.Subscribe<TaskListSortOrderChangedMsg>(msg =>
                {
                    SortTaskLists(msg.NewSortOrder);
                    TaskListsWereLoaded(false);
                }),
                Messenger.Subscribe<TaskListSavedMsg>(msg =>
                {
                    TaskLists.Add(msg.TaskList);
                    SelectedTaskList = msg.TaskList;
                    AppSettings.SelectedTaskListId = SelectedTaskList.GoogleId;
                    TaskListsWereLoaded(false);
                }),
                Messenger.Subscribe<RefreshNumberOfTasksMsg>(UpdateNumberOfTasks),
                Messenger.Subscribe<OnFullSyncMsg>(async msg =>
                {
                    await InitView(true);
                })
            };

            SubscriptionTokens.AddRange(tokens);
        }

        private async Task LoadProfileInfo()
        {
            var userResponse = await _dataService.UserService.GetCurrentActiveUserAsync();

            if (!userResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(LoadProfileInfo)}: Current active user couldnt be loaded. " +
                    $"Error = {userResponse.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                return;
            }

            CurrentUserName = userResponse.Result.Fullname;
            CurrentUserEmail = userResponse.Result.Email;
            string imgPath = MiscellaneousUtils.GetUserProfileImagePath(userResponse.Result.GoogleUserID);
            _onUserProfileImgLoaded.Raise(imgPath);
        }

        private async Task InitView(bool onFullSync = false)
        {
            string selectedTaskListID = null;

            if (!onFullSync && AppSettings.RunSyncBackgroundTaskAfterStart)
            {
                _dialogService.ShowSnackBar("Running a full sync...");
                _backgroundTaskManager.StartBackgroundTask(BackgroundTaskType.SYNC);
                return;
            }

            //If we have something in the init params, lets select that task list
            selectedTaskListID = !onFullSync && InitParams != null
                ? InitParams.TaskListId
                : AppSettings.SelectedTaskListId;

            TaskLists.Clear();
            var dbResponse = await _dataService
                .TaskListService
                .GetAsNoTrackingAsync(
                    tl => tl.User.IsActive && tl.LocalStatus != LocalStatus.DELETED,
                    includeProperties: nameof(GoogleTaskList.Tasks));

            if (!dbResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(InitView)}: An unknown error occurred while trying to retrieve all " +
                    $"the task lists from the db. Error = {dbResponse.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                return;
            }

            var taskLists = _mapper.Map<List<TaskListItemViewModel>>(dbResponse.Result);
            foreach (var taskList in taskLists)
            {
                taskList.NumberOfTasks = dbResponse.Result
                    .First(tl => tl.GoogleTaskListID == taskList.GoogleId)
                    .Tasks
                    .Count();
            }

            TaskLists.AddRange(taskLists);

            SortTaskLists(AppSettings.DefaultTaskListSortOrder);

            SelectedTaskList = TaskLists.Any(tl => tl.GoogleId == selectedTaskListID)
                ? TaskLists.First(tl => tl.GoogleId == selectedTaskListID)
                : TaskLists.FirstOrDefault();

            AppSettings.SelectedTaskListId = SelectedTaskList.GoogleId;

            TaskListsWereLoaded(Parameter.OrientationChanged);
        }

        private void SortTaskLists(TaskListSortType sortType)
        {
            if (TaskLists.Count == 0)
                return;

            switch (sortType)
            {
                case TaskListSortType.BY_NAME_ASC:
                    TaskLists.SortBy(tl => tl.Title);
                    break;
                case TaskListSortType.BY_NAME_DESC:
                    TaskLists.SortByDescending(tl => tl.Title);
                    break;
                case TaskListSortType.BY_UPDATED_DATE_ASC:
                    TaskLists.SortBy(tl => tl.UpdatedAt);
                    break;
                case TaskListSortType.BY_UPDATED_DATE_DESC:
                    TaskLists.SortByDescending(tl => tl.UpdatedAt);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortType), sortType,
                        "The provided task list sort type does not exists");
            }
        }

        private async Task OnTaskListSelected(int position)
        {
            var taskList = TaskLists[position];
            SelectedTaskList = taskList;
            AppSettings.SelectedTaskListId = SelectedTaskList.GoogleId;

            var parameter = TasksViewModelParameter.Instance(InitParams, taskList);
            var tasks = new List<Task>
            {
                Task.Delay(300),
                NavigationService.Navigate<TasksViewModel, TasksViewModelParameter>(parameter)
            };
            await Task.WhenAll(tasks);
        }

        private void UpdateNumberOfTasks(RefreshNumberOfTasksMsg msg)
        {
            if (msg.WasAdded)
                SelectedTaskList.NumberOfTasks += msg.AffectedItems;
            else
                SelectedTaskList.NumberOfTasks -= msg.AffectedItems;

            if (SelectedTaskList.NumberOfTasks < 0)
                SelectedTaskList.NumberOfTasks = 0;

            int position = TaskLists.IndexOf(SelectedTaskList);
            _refreshNumberOfTasks.Raise(position);

            if (msg.TaskWasMoved && TaskLists.Any(tl => tl.GoogleId == msg.MovedToTaskListId))
            {
                var taskList = TaskLists.First(tl => tl.GoogleId == msg.MovedToTaskListId);
                taskList.NumberOfTasks += msg.AffectedItems;

                int newPosition = TaskLists.IndexOf(taskList);
                _refreshNumberOfTasks.Raise(newPosition);
            }
        }

        private void TaskListsWereLoaded(bool orientationChanged)
        {
            _onTaskListsLoaded.Raise(orientationChanged);
        }
    }
}