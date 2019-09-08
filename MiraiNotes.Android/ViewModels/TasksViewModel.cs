using AutoMapper;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MiraiNotes.Shared.Extensions;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels
{
    public class TasksViewModel : BaseViewModel<TaskListItemViewModel>
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IUserCredentialService _userCredentialService;

        private TaskListItemViewModel _currentTaskList;
        private MvxObservableCollection<TaskItemViewModel> _tasks = new MvxObservableCollection<TaskItemViewModel>();
        private bool _isBusy;
        private TaskSortType _currentTasksSortOrder = TaskSortType.BY_NAME_ASC;
        private bool _showProgressOverlay;

        public MvxObservableCollection<TaskItemViewModel> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public TaskSortType CurrentTasksSortOrder
        {
            get => _currentTasksSortOrder;
            set => SetProperty(ref _currentTasksSortOrder, value);
        }

        public bool ShowProgressOverlay
        {
            get => _showProgressOverlay;
            set => SetProperty(ref _showProgressOverlay, value);
        }

        public IMvxAsyncCommand<TaskItemViewModel> TaskSelectedCommand { get; private set; }
        public IMvxAsyncCommand RefreshTasksCommand { get; private set; }
        public IMvxCommand AddNewTaskListCommand { get; private set; }
        public IMvxAsyncCommand AddNewTaskCommand { get; private set; }

        public TasksViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            IMapper mapper,
            IDialogService dialogService,
            IMiraiNotesDataService dataService,
            IAppSettingsService appSettings,
            IGoogleApiService googleApiService,
            IUserCredentialService userCredentialService)
            : base(textProvider, messenger, appSettings)
        {
            _navigationService = navigationService;
            _mapper = mapper;
            _dialogService = dialogService;
            _dataService = dataService;
            _googleApiService = googleApiService;
            _userCredentialService = userCredentialService;

            SetCommands();
            RegisterMessages();
        }

        public override void Prepare(TaskListItemViewModel taskList)
        {
            _currentTaskList = taskList;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await InitView(_currentTaskList);
        }

        private void SetCommands()
        {
            TaskSelectedCommand = new MvxAsyncCommand<TaskItemViewModel>((task) => OnTaskSelected(task.TaskID));
            RefreshTasksCommand = new MvxAsyncCommand(Refresh);
            AddNewTaskListCommand = new MvxCommand(() => _dialogService.ShowSnackBar("not implemented", string.Empty));
            AddNewTaskCommand = new MvxAsyncCommand(() => OnTaskSelected(string.Empty));
        }

        private void RegisterMessages()
        {
            var subscriptions = new[] {
                Messenger.Subscribe<TaskDeletedMsg>(OnTaskDeleted),
                Messenger.Subscribe<TaskSavedMsg>(async msg => await OnTaskSaved(msg)),
                Messenger.Subscribe<TaskStatusChangedMsg>(OnTaskStatusChanged),
                Messenger.Subscribe<ShowTasksLoadingMsg>(msg => IsBusy = msg.Show),
                Messenger.Subscribe<ShowProgressOverlayMsg>(msg => ShowProgressOverlay = msg.Show),
                Messenger.Subscribe<TaskSortOrderChangedMsg>(msg => SortTasks(msg.NewSortOrder))
            };

            SubscriptionTokens.AddRange(subscriptions);
        }

        public async Task InitView(TaskListItemViewModel taskList)
        {
            if (taskList == null)
            {
                //OnNoTaskListAvailable();
                return;
            }
            IsBusy = true;

            Tasks.Clear();
            //TaskAutoSuggestBoxItems.Clear();

            var dbResponse = await _dataService
                .TaskService
                .GetAsNoTrackingAsync(
                    t => t.TaskList.GoogleTaskListID == taskList.Id &&
                         t.LocalStatus != LocalStatus.DELETED,
                    t => t.OrderBy(ta => ta.Position));

            if (!dbResponse.Succeed)
            {
                _dialogService.ShowErrorToast(
                    $"An unknown error occurred while trying to retrieve all the tasks from db. Error = {dbResponse.Message}");
                IsBusy = false;
                return;
            }

            var tasks = _mapper.Map<List<TaskItemViewModel>>(dbResponse.Result);
            if (tasks.Any())
            {
                var mainTasks = tasks
                    .Where(t => t.ParentTask == null);
                mainTasks.ForEach(t =>
                {
                    if (!tasks.Any(st => st.ParentTask == t.TaskID))
                        return;
                    t.SubTasks = _mapper.Map<MvxObservableCollection<TaskItemViewModel>>(
                        tasks
                            .Where(st => st.ParentTask == t.TaskID)
                            .OrderBy(st => st.Position));
                });
                Tasks.AddRange(mainTasks);
                //TaskAutoSuggestBoxItems
                //    .AddRange(_mapper.Map<IEnumerable<ItemModel>>(mainTasks.OrderBy(t => t.Title)));

                SortTasks(AppSettings.DefaultTaskSortOrder);
            }

            //CurrentTaskList = taskList;

            //If we have something in the init details, lets select that task
            //if (InitDetails is null == false &&
            //    !string.IsNullOrEmpty(InitDetails.Item1) &&
            //    !string.IsNullOrEmpty(InitDetails.Item2))
            //{
            //    var selectedTask = Tasks.FirstOrDefault(t => t.TaskID == InitDetails.Item2);
            //    if (selectedTask is null == false)
            //        selectedTask.IsSelected = true;
            //    InitDetails = null;
            //}

            IsBusy = false;
        }

        public async Task OnTaskSelected(string taskId)
            => await _navigationService.Navigate<NewTaskViewModel, Tuple<TaskListItemViewModel, string>>(
                new Tuple<TaskListItemViewModel, string>(_currentTaskList, taskId));

        public async Task OnTaskSaved(TaskSavedMsg msg)
        {
            IsBusy = true;
            var dbResponse = await _dataService
                .TaskService
                .FirstOrDefaultAsNoTrackingAsync(ta => ta.GoogleTaskID == msg.TaskId);

            if (!dbResponse.Succeed || dbResponse.Result == null)
            {
                IsBusy = false;
                string errorMsg = dbResponse.Result == null
                    ? "Could not find the saved task in db"
                    : $"An unknown error occurred. Error = {dbResponse.Message}";
                _dialogService.ShowErrorToast(errorMsg);
                return;
            }

            var task = _mapper.Map<TaskItemViewModel>(dbResponse.Result);

            if (!task.HasParentTask)
            {
                var stsResponse = await _dataService
                    .TaskService
                    .GetAsNoTrackingAsync(
                        st => st.ParentTask == task.TaskID,
                        st => st.OrderBy(s => s.Position));

                if (!stsResponse.Succeed)
                {
                    IsBusy = false;
                    _dialogService.ShowErrorToast(
                        $"An unknown error occurred. Error = {dbResponse.Message}");
                    return;
                }

                task.SubTasks = _mapper.Map<MvxObservableCollection<TaskItemViewModel>>(stsResponse.Result);
            }

            IsBusy = false;

            if (task.HasParentTask)
            {
                task.IsSelected = true;
                var parentTask = Tasks?
                    .FirstOrDefault(t => t.TaskID == task.ParentTask);

                if (parentTask == null)
                    return;

                int updatedSubTaskIndex = parentTask
                                              .SubTasks?
                                              .ToList()
                                              .FindIndex(st => st.TaskID == task.TaskID) ?? -1;

                if (updatedSubTaskIndex != -1)
                    parentTask.SubTasks[updatedSubTaskIndex] = task;
                else
                    parentTask.SubTasks.Add(task);
            }
            else
            {
                int updatedTaskIndex = Tasks?
                                           .ToList()
                                           .FindIndex(t => t.TaskID == task.TaskID) ?? -1;

                if (updatedTaskIndex >= 0)
                {
                    task.IsSelected = true;
                    Tasks[updatedTaskIndex] = task;
                }
                else
                {
                    Tasks.Add(task);
                }
            }
        }

        public void OnTaskStatusChanged(TaskStatusChangedMsg msg)
        {
            TaskItemViewModel taskFound;
            if (!msg.HasParentTask)
            {
                taskFound = Tasks
                    .FirstOrDefault(t => t.TaskID == msg.TaskId);
            }
            else
            {
                taskFound = Tasks
                    .FirstOrDefault(t => t.TaskID == msg.ParentTask)?
                    .SubTasks?
                    .FirstOrDefault(st => st.TaskID == msg.TaskId);
            }

            if (taskFound == null)
                return;

            taskFound.CompletedOn = msg.CompletedOn;
            taskFound.UpdatedAt = msg.UpdatedAt;
            taskFound.Status = msg.NewStatus;
        }

        public void OnTaskDeleted(TaskDeletedMsg msg)
        {
            if (!msg.HasParentTask)
            {
                Tasks.RemoveAll(t => t.TaskID == msg.TaskId);
            }
            else
            {
                Tasks
                    .FirstOrDefault(t => t.TaskID == msg.ParentTask)?
                    .SubTasks?
                    .RemoveAll(st => st.TaskID == msg.TaskId);
            }
        }

        private async Task Refresh()
        {
            IsBusy = true;
            await Task.Delay(2000);
            // do refresh work here
            IsBusy = false;
        }

        private void SortTasks(TaskSortType sortType)
        {
            //TODO: WHEN YOU SORT, THE SELECTED ITEM GETS LOST
            if (Tasks == null)
                return;

            //_isSelectionInProgress = true;
            switch (sortType)
            {
                case TaskSortType.BY_NAME_ASC:
                    Tasks.SortBy(t => t.Title);
                    break;
                case TaskSortType.BY_NAME_DESC:
                    Tasks.SortByDescending(t => t.Title);
                    break;
                case TaskSortType.BY_UPDATED_DATE_ASC:
                    Tasks.SortBy(t => t.UpdatedAt);
                    break;
                case TaskSortType.BY_UPDATED_DATE_DESC:
                    Tasks.SortByDescending(t => t.UpdatedAt);
                    break;
                case TaskSortType.CUSTOM_ASC:
                    Tasks.SortBy(t => t.Position);
                    break;
                case TaskSortType.CUSTOM_DESC:
                    Tasks.SortByDescending(t => t.UpdatedAt);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortType),
                        "The TaskSortType doesnt have a default sort type");
            }

            CurrentTasksSortOrder = sortType;
            //_isSelectionInProgress = false;
        }
    }
}