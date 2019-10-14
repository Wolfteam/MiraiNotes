using AutoMapper;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Android.ViewModels.Dialogs;
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
    public class TasksViewModel : BaseViewModel<TasksViewModelParameter>
    {
        #region Members
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IUserCredentialService _userCredentialService;

        private TaskListItemViewModel _currentTaskList;
        private MvxObservableCollection<TaskItemViewModel> _tasks = new MvxObservableCollection<TaskItemViewModel>();
        private bool _isBusy;
        private TaskSortType _currentTasksSortOrder = TaskSortType.BY_NAME_ASC;
        #endregion

        #region Properties
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

        public NotificationAction InitParams { get; set; }
        #endregion

        #region Commands
        public IMvxAsyncCommand<TaskItemViewModel> TaskSelectedCommand { get; private set; }
        public IMvxAsyncCommand RefreshTasksCommand { get; private set; }
        public IMvxAsyncCommand AddNewTaskListCommand { get; private set; }
        public IMvxAsyncCommand AddNewTaskCommand { get; private set; }
        public IMvxAsyncCommand<TaskItemViewModel> ShowMenuOptionsDialogCommand { get; private set; }
        #endregion

        public TasksViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IMapper mapper,
            IDialogService dialogService,
            IMiraiNotesDataService dataService,
            IAppSettingsService appSettings,
            IGoogleApiService googleApiService,
            IUserCredentialService userCredentialService,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<TasksViewModel>(), navigationService, appSettings, telemetryService)
        {
            _mapper = mapper;
            _dialogService = dialogService;
            _dataService = dataService;
            _googleApiService = googleApiService;
            _userCredentialService = userCredentialService;
        }

        public override void Prepare(TasksViewModelParameter parameter)
        {
            InitParams = parameter.NotificationAction;
            _currentTaskList = parameter.TaskList;
            Title = GetText("Tasks");
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await InitView(_currentTaskList);
        }

        public override void SetCommands()
        {
            base.SetCommands();
            TaskSelectedCommand = new MvxAsyncCommand<TaskItemViewModel>((task) => OnTaskSelected(task.GoogleId));
            RefreshTasksCommand = new MvxAsyncCommand(Refresh);
            AddNewTaskListCommand = new MvxAsyncCommand(() => NavigationService.Navigate<TaskListDialogViewModel>());
            AddNewTaskCommand = new MvxAsyncCommand(() => OnTaskSelected(string.Empty));
            ShowMenuOptionsDialogCommand = new MvxAsyncCommand<TaskItemViewModel>((task) =>
            {
                var parameter = TaskMenuOptionsViewModelParameter.Instance(_currentTaskList, task);
                return NavigationService.Navigate<TaskMenuOptionsViewModel, TaskMenuOptionsViewModelParameter>(parameter);
            });
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
            var subscriptions = new[] {
                Messenger.Subscribe<TaskDeletedMsg>(msg => OnTaskDeleted(msg.TaskId, msg.ParentTask, msg.HasParentTask)),
                Messenger.Subscribe<TaskSavedMsg>(async msg => await OnTaskSaved(msg.TaskId, msg.ItemsAdded)),
                Messenger.Subscribe<TaskStatusChangedMsg>(OnTaskStatusChanged),
                Messenger.Subscribe<ShowTasksLoadingMsg>(msg => IsBusy = msg.Show),
                Messenger.Subscribe<TaskSortOrderChangedMsg>(msg => SortTasks(msg.NewSortOrder)),
                Messenger.Subscribe<DeleteTaskRequestMsg>(async msg => await DeleteTask(msg.Task)),
                Messenger.Subscribe<ChangeTaskStatusRequestMsg>(async msg => await ChangeTaskStatus(msg.Task, msg.NewStatus)),
                Messenger.Subscribe<TaskDateUpdatedMsg>(msg => OnTaskDateUpdated(msg.Task, msg.IsAReminderDate)),
                Messenger.Subscribe<TaskMovedMsg>(msg => OnTaskDeleted(msg.TaskId, msg.ParentTask, msg.HasParentTask, msg.NewTaskListId)),
                Messenger.Subscribe<SubTaskSelectedMsg>(async msg => await OnSubTaskSelected(msg))
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

            var dbResponse = await _dataService
                .TaskService
                .GetAsNoTrackingAsync(
                    t => t.TaskList.GoogleTaskListID == taskList.GoogleId &&
                         t.LocalStatus != LocalStatus.DELETED);

            if (!dbResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(InitView)}: An unknown error occurred while trying to retrieve all the tasks from db." +
                    $"Error = {dbResponse.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                IsBusy = false;
                return;
            }

            var tasks = _mapper.Map<List<TaskItemViewModel>>(dbResponse.Result);
            if (tasks.Any())
            {
                var mainTasks = tasks
                    .Where(t => t.ParentTask == null);

                foreach (var t in mainTasks)
                {
                    if (!tasks.Any(st => st.ParentTask == t.GoogleId))
                        continue;
                    t.SubTasks = _mapper.Map<MvxObservableCollection<TaskItemViewModel>>(
                        tasks
                            .Where(st => st.ParentTask == t.GoogleId)
                            .OrderBy(st => st.Position));
                }
                Tasks.AddRange(mainTasks);
                //TaskAutoSuggestBoxItems
                //    .AddRange(_mapper.Map<IEnumerable<ItemModel>>(mainTasks.OrderBy(t => t.Title)));

                SortTasks(AppSettings.DefaultTaskSortOrder);
            }

            //CurrentTaskList = taskList;
            IsBusy = false;

            //If we have something in the init params, lets select that task
            if (InitParams != null && Tasks.Any(t => t.GoogleId == InitParams.TaskId))
            {
                string taskId = InitParams.TaskId;
                InitParams = null;
                await OnTaskSelected(taskId);
            }
        }

        public async Task OnTaskSelected(string taskId)
        {
            var parameter = NewTaskViewModelParameter.Instance(_currentTaskList, taskId);
            await NavigationService.Navigate<NewTaskViewModel, NewTaskViewModelParameter>(parameter);
        }

        public async Task OnTaskSaved(string taskId, int itemsAdded)
        {
            Messenger.Publish(new RefreshNumberOfTasksMsg(this, true, itemsAdded));

            IsBusy = true;
            var dbResponse = await _dataService
                .TaskService
                .FirstOrDefaultAsNoTrackingAsync(ta => ta.GoogleTaskID == taskId);

            if (!dbResponse.Succeed || dbResponse.Result == null)
            {
                IsBusy = false;
                string errorMsg = dbResponse.Result == null
                    ? "Could not find the saved task in db"
                    : $"An unknown error occurred. Error = {dbResponse.Message}";

                Logger.Error(
                    $"{nameof(OnTaskSaved)}: {errorMsg}." +
                    $"Error = {dbResponse.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                return;
            }

            var task = _mapper.Map<TaskItemViewModel>(dbResponse.Result);

            if (!task.HasParentTask)
            {
                var stsResponse = await _dataService
                    .TaskService
                    .GetAsNoTrackingAsync(
                        st => st.ParentTask == task.GoogleId,
                        st => st.OrderBy(s => s.Position));

                if (!stsResponse.Succeed)
                {
                    IsBusy = false;
                    Logger.Error(
                        $"{nameof(OnTaskSaved)}: An error occurred while trying to retrieve sub tasks" +
                        $"Error = {stsResponse.Message}");
                    _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                    return;
                }

                task.SubTasks = _mapper.Map<MvxObservableCollection<TaskItemViewModel>>(stsResponse.Result);
            }

            IsBusy = false;

            if (task.HasParentTask)
            {
                task.IsSelected = true;
                var parentTask = Tasks?
                    .FirstOrDefault(t => t.GoogleId == task.ParentTask);

                if (parentTask == null)
                    return;

                int updatedSubTaskIndex = parentTask
                                              .SubTasks?
                                              .ToList()
                                              .FindIndex(st => st.GoogleId == task.GoogleId) ?? -1;

                if (updatedSubTaskIndex != -1)
                    parentTask.SubTasks[updatedSubTaskIndex] = task;
                else
                    parentTask.SubTasks.Add(task);
            }
            else
            {
                int updatedTaskIndex = Tasks?
                                           .ToList()
                                           .FindIndex(t => t.GoogleId == task.GoogleId) ?? -1;

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

        public void OnTaskDateUpdated(TaskItemViewModel task, bool isAReminderDate)
        {
            TaskItemViewModel updatedTask = null;

            if (!task.HasParentTask)
            {
                updatedTask = Tasks.FirstOrDefault(t => t.GoogleId == task.GoogleId);
            }
            else
            {
                Tasks
                    .FirstOrDefault(t => t.GoogleId == task.ParentTask)?
                    .SubTasks?
                    .FirstOrDefault(st => st.GoogleId == task.GoogleId);
            }

            if (updatedTask is null)
                return;

            if (isAReminderDate)
            {
                updatedTask.RemindOn = task.RemindOn;
                updatedTask.RemindOnGUID = task.RemindOnGUID;
            }
            else
            {
                updatedTask.ToBeCompletedOn = task.ToBeCompletedOn;
            }
        }

        public void OnTaskStatusChanged(TaskStatusChangedMsg msg)
        {
            TaskItemViewModel taskFound;
            if (!msg.HasParentTask)
            {
                taskFound = Tasks
                    .FirstOrDefault(t => t.GoogleId == msg.TaskId);
            }
            else
            {
                taskFound = Tasks
                    .FirstOrDefault(t => t.GoogleId == msg.ParentTask)?
                    .SubTasks?
                    .FirstOrDefault(st => st.GoogleId == msg.TaskId);
            }

            if (taskFound == null)
                return;

            taskFound.CompletedOn = msg.CompletedOn;
            taskFound.UpdatedAt = msg.UpdatedAt;
            taskFound.Status = msg.NewStatus;
        }

        public void OnTaskDeleted(string taskId, string parentTask, bool hasParentTask)
            => OnTaskDeleted(taskId, parentTask, hasParentTask, null);

        public void OnTaskDeleted(string taskId, string parentTask, bool hasParentTask, string taskListId)
        {
            int affectedItems = 1;
            if (!hasParentTask)
            {
                var task = Tasks.First(t => t.GoogleId == taskId);
                affectedItems += task.SubTasks.Count;
                Tasks.Remove(task);
            }
            else
            {
                Tasks
                    .FirstOrDefault(t => t.GoogleId == parentTask)?
                    .SubTasks?
                    .RemoveAll(st => st.GoogleId == taskId);
            }
            Messenger.Publish(new RefreshNumberOfTasksMsg(this, affectedItems, taskListId));
        }

        private async Task OnSubTaskSelected(SubTaskSelectedMsg msg)
        {
            if (msg.ShowMenuOptions)
            {
                await ShowMenuOptionsDialogCommand.ExecuteAsync(msg.SubTask);
            }
            else
            {
                await OnTaskSelected(msg.SubTask.GoogleId);
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

        private async Task DeleteTask(TaskItemViewModel task)
        {
            Messenger.Publish(new ShowProgressOverlayMsg(this));

            var deleteResponse = await _dataService
                .TaskService
                .RemoveTaskAsync(task.GoogleId);

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            if (!deleteResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(DeleteTask)}: Couldn't delete the selected task." +
                    $"Error = {deleteResponse.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                return;
            }
            OnTaskDeleted(task.GoogleId, task.ParentTask, task.HasParentTask);
        }

        private async Task ChangeTaskStatus(TaskItemViewModel task, GoogleTaskStatus newStatus)
        {
            string statusMessage =
                $"{(newStatus == GoogleTaskStatus.COMPLETED ? GetText("Completed") : GetText("Incompleted"))}";

            Messenger.Publish(new ShowProgressOverlayMsg(this));

            var response = await _dataService
                .TaskService
                .ChangeTaskStatusAsync(task.GoogleId, newStatus);

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            if (!response.Succeed)
            {
                Logger.Error(
                    $"{nameof(ChangeTaskStatus)}: An error occurred while trying to mark {task.Title} as {statusMessage}." +
                    $"Error = {response.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                return;
            }

            task.Status = response.Result.Status;
            task.CompletedOn = response.Result.CompletedOn;
            task.UpdatedAt = response.Result.UpdatedAt;

            var msg = new TaskStatusChangedMsg(
                this,
                task.GoogleId,
                task.ParentTask,
                task.CompletedOn,
                task.UpdatedAt,
                task.Status);
            OnTaskStatusChanged(msg);

            _dialogService.ShowSnackBar(GetText("TaskStatusChanged", task.Title, statusMessage));
        }
    }
}