using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MiraiNotes.Shared.Extensions;
using MiraiNotes.Shared.Helpers;
using MiraiNotes.Shared.Utils;
using MiraiNotes.UWP.Delegates;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiraiNotes.UWP.ViewModels
{
    public class TasksPageViewModel : BaseViewModel
    {
        #region Members

        private readonly ICustomDialogService _dialogService;
        private readonly IMessenger _messenger;
        private readonly IMapper _mapper;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IBackgroundTaskManagerService _bgTaskManagerService;
        private readonly IAppSettingsService _appSettings;
        private readonly INotificationService _notificationService;

        private TaskListItemViewModel _currentTaskList;

        private SmartObservableCollection<TaskItemViewModel>
            _tasks = new SmartObservableCollection<TaskItemViewModel>();

        private SmartObservableCollection<ItemModel> _taskAutoSuggestBoxItems =
            new SmartObservableCollection<ItemModel>();

        private bool _isTaskListTitleVisible;
        private bool _isTaskListViewVisible;
        private bool _isAutoSuggestBoxEnabled;
        private bool _canAddMoreTaskList;
        private bool _canAddMoreTasks;
        private bool _canDeleteTasks;
        private bool _canRefreshTaskListView;
        private bool _canSortTaskListView;
        private bool _showTaskListViewProgressRing;
        private bool _isTaskListCommandBarCompact;
        private string _selectedTasksText;
        private string _taskAutoSuggestBoxText;

        private bool _isSelectionInProgress;

        private ObservableCollection<TaskListItemViewModel> _taskLists =
            new ObservableCollection<TaskListItemViewModel>();

        private int _selectedTaskIndex;

        private bool _showMoveTaskFlyoutProgressBar;

        private TaskSortType _currentTasksSortOrder = TaskSortType.BY_NAME_ASC;

        public int DesiredTaskIndex = -1;
        #endregion

        #region Properties

        public SmartObservableCollection<TaskItemViewModel> Tasks
        {
            get { return _tasks; }
            set { _tasks = value; }
        }

        public int SelectedTaskIndex
        {
            get => _selectedTaskIndex;
            set => Set(ref _selectedTaskIndex, value);
        }

        public SmartObservableCollection<ItemModel> TaskAutoSuggestBoxItems
        {
            get { return _taskAutoSuggestBoxItems; }
            set { _taskAutoSuggestBoxItems = value; }
        }

        public TaskListItemViewModel CurrentTaskList
        {
            get { return _currentTaskList; }
            set { Set(ref _currentTaskList, value); }
        }

        public bool IsTaskListTitleVisible
        {
            get { return _isTaskListTitleVisible; }
            set { Set(ref _isTaskListTitleVisible, value); }
        }

        public bool IsTaskListViewVisible
        {
            get { return _isTaskListViewVisible; }
            set { Set(ref _isTaskListViewVisible, value); }
        }

        public bool IsAutoSuggestBoxEnabled
        {
            get { return _isAutoSuggestBoxEnabled; }
            set { Set(ref _isAutoSuggestBoxEnabled, value); }
        }

        public bool CanAddMoreTaskList
        {
            get { return _canAddMoreTaskList; }
            set { Set(ref _canAddMoreTaskList, value); }
        }

        public bool CanAddMoreTasks
        {
            get { return _canAddMoreTasks; }
            set { Set(ref _canAddMoreTasks, value); }
        }

        public bool CanDeleteTasks
        {
            get { return _canDeleteTasks; }
            set { Set(ref _canDeleteTasks, value); }
        }

        public bool CanRefreshTaskListView
        {
            get { return _canRefreshTaskListView; }
            set { Set(ref _canRefreshTaskListView, value); }
        }

        public bool CanSortTaskListView
        {
            get { return _canSortTaskListView; }
            set { Set(ref _canSortTaskListView, value); }
        }

        public bool IsTaskListCommandBarCompact
        {
            get { return _isTaskListCommandBarCompact; }
            set { Set(ref _isTaskListCommandBarCompact, value); }
        }

        public bool ShowTaskListViewProgressRing
        {
            get { return _showTaskListViewProgressRing; }
            set
            {
                HideControls(!value);
                DisableControls(!value);
                Set(ref _showTaskListViewProgressRing, value);
            }
        }

        public string SelectedTasksText
        {
            get { return _selectedTasksText; }
            set { Set(ref _selectedTasksText, value); }
        }

        public string TaskAutoSuggestBoxText
        {
            get { return _taskAutoSuggestBoxText; }
            set { Set(ref _taskAutoSuggestBoxText, value); }
        }

        public TaskSortType CurrentTasksSortOrder
        {
            get { return _currentTasksSortOrder; }
            set { Set(ref _currentTasksSortOrder, value); }
        }

        public ObservableCollection<TaskListItemViewModel> TaskLists
        {
            get { return _taskLists; }
            set { Set(ref _taskLists, value); }
        }

        public TaskItemViewModel SelectedTaskToMove { get; set; }

        public bool ShowMoveTaskFlyoutProgressBar
        {
            get { return _showMoveTaskFlyoutProgressBar; }
            set { Set(ref _showMoveTaskFlyoutProgressBar, value); }
        }

        #endregion

        #region Commands

        public ICommand NewTaskCommand { get; set; }

        public ICommand NewTaskListCommand { get; set; }

        public ICommand TaskListViewSelectedItemCommand { get; set; }

        public ICommand TaskAutoSuggestBoxTextChangedCommand { get; set; }

        public ICommand TaskAutoSuggestBoxQuerySubmittedCommand { get; set; }

        public ICommand SelectAllTaskCommand { get; set; }

        public ICommand DeleteTaskCommand { get; set; }

        public ICommand DeleteSelectedTasksCommand { get; set; }

        public ICommand MarkAsCompletedCommand { get; set; }

        public ICommand MarkAsIncompletedCommand { get; set; }

        public ICommand MarkAsCompletedSelectedTasksCommand { get; set; }

        public ICommand SyncCommand { get; set; }

        public ICommand SortTasksCommand { get; set; }

        public ICommand MoveComboBoxClickedCommand { get; set; }

        public ICommand MoveComboBoxSelectionChangedCommand { get; set; }

        public ICommand MoveComboBoxOpenedCommand { get; set; }

        public ICommand MoveSelectedTasksCommand { get; set; }

        public ICommand ShowSubTasksCommand { get; set; }

        public ICommand SubTaskSelectedItemCommand { get; set; }

        #endregion

        #region Events

        public ShowInAppNotificationRequest InAppNotificationRequest { get; set; }

        #endregion

        #region Constructors

        public TasksPageViewModel(
            ICustomDialogService dialogService,
            IMessenger messenger,
            IMapper mapper,
            IMiraiNotesDataService dataService,
            IBackgroundTaskManagerService bgTaskManagerService,
            IAppSettingsService appSettings,
            INotificationService notificationService)
        {
            _dialogService = dialogService;
            _messenger = messenger;
            _mapper = mapper;
            _dataService = dataService;
            _bgTaskManagerService = bgTaskManagerService;
            _appSettings = appSettings;
            _notificationService = notificationService;

            RegisterMessages();
            SetCommands();
            IsTaskListCommandBarCompact = true;
        }

        #endregion

        #region Methods

        private void RegisterMessages()
        {
            _messenger.Register<TaskListItemViewModel>(
                this,
                $"{MessageType.NAVIGATIONVIEW_SELECTION_CHANGED}",
                async (taskList) => await GetAllTasksAsync(taskList));
            _messenger.Register<bool>(
                this,
                $"{MessageType.SHOW_CONTENT_FRAME_PROGRESS_RING}",
                (show) => ShowTaskListViewProgressRing = show);
            _messenger.Register<string>(
                this,
                $"{MessageType.TASK_SAVED}",
                async (taskID) => await OnTaskSavedAsync(taskID));
            _messenger.Register<string>(this, $"{MessageType.TASK_DELETED_FROM_PANE_FRAME}", OnTaskDeleted);
            _messenger.Register<KeyValuePair<string, string>>(
                this,
                $"{MessageType.SUBTASK_DELETED_FROM_PANE_FRAME}",
                (kvp) => OnSubTaskDeleted(kvp.Key, kvp.Value));
            _messenger.Register<Tuple<TaskItemViewModel, bool>>(
                this,
                $"{MessageType.TASK_STATUS_CHANGED_FROM_PANE_FRAME}",
                (tuple) => OnTaskStatusChanged(tuple.Item1, tuple.Item2));
            _messenger.Register<string>(
                this,
                $"{MessageType.SHOW_IN_APP_NOTIFICATION}",
                (message) => InAppNotificationRequest?.Invoke(message));
            _messenger.Register<TaskSortType>(
                this,
                $"{MessageType.DEFAULT_TASK_SORT_ORDER_CHANGED}",
                SortTasks);
            _messenger.Register<string>(
                this,
                $"{MessageType.TASK_CHANGES_WERE_DISCARDED}",
                taskId =>
                {
                    if (DesiredTaskIndex != -1)
                        SelectedTaskIndex = DesiredTaskIndex;
                    DesiredTaskIndex = -1;
                });
        }

        private void SetCommands()
        {
            TaskListViewSelectedItemCommand = new RelayCommand<TaskItemViewModel>
                ((task) => OnTaskSelectectionChanged(task));

            TaskAutoSuggestBoxTextChangedCommand = new RelayCommand<string>
                ((text) => OnTaskAutoSuggestBoxTextChangeAsync(text));

            TaskAutoSuggestBoxQuerySubmittedCommand = new RelayCommand<ItemModel>
                (OnTaskAutoSuggestBoxQuerySubmitted);

            NewTaskCommand = new RelayCommand(NewTask);

            NewTaskListCommand = new RelayCommand
                (async () => await SaveNewTaskListAsync());

            DeleteTaskCommand = new RelayCommand<TaskItemViewModel>
                (async (task) => await DeleteTaskAsync(task));

            DeleteSelectedTasksCommand = new RelayCommand
                (async () => await DeleteSelectedTasksAsync());

            MarkAsCompletedCommand = new RelayCommand<TaskItemViewModel>
                (async (task) => await ChangeTaskStatusAsync(task, GoogleTaskStatus.COMPLETED, task.HasParentTask));

            MarkAsIncompletedCommand = new RelayCommand<TaskItemViewModel>
                (async (task) => await ChangeTaskStatusAsync(task, GoogleTaskStatus.NEEDS_ACTION, task.HasParentTask));

            MarkAsCompletedSelectedTasksCommand = new RelayCommand
                (async () => await ChangeSelectedTasksStatusAsync(GoogleTaskStatus.COMPLETED));

            SyncCommand = new RelayCommand
                (() => _bgTaskManagerService.StartBackgroundTask(BackgroundTaskType.SYNC));

            SortTasksCommand = new RelayCommand<TaskSortType>
                ((sortBy) => SortTasks(sortBy));

            SelectAllTaskCommand = new RelayCommand
                (() => MarkAsSelectedAllTasks(true));

            MoveComboBoxClickedCommand = new RelayCommand<TaskItemViewModel>
                ((clickedItem) => SelectedTaskToMove = clickedItem);

            MoveComboBoxSelectionChangedCommand = new RelayCommand<TaskListItemViewModel>(async (selectedTaskList) =>
            {
                if (selectedTaskList == null || SelectedTaskToMove == null)
                    return;
                await MoveTaskAsync(SelectedTaskToMove, selectedTaskList);
            });

            MoveComboBoxOpenedCommand = new RelayCommand(async () =>
            {
                ShowMoveTaskFlyoutProgressBar = true;
                var dbResponse = await _dataService
                    .TaskListService
                    .GetAsNoTrackingAsync(
                        tl => tl.LocalStatus != LocalStatus.DELETED &&
                              tl.GoogleTaskListID != _currentTaskList.TaskListID &&
                              tl.User.IsActive,
                        tl => tl.OrderBy(t => t.Title));

                TaskLists = _mapper.Map<ObservableCollection<TaskListItemViewModel>>(dbResponse.Result);
                ShowMoveTaskFlyoutProgressBar = false;
            });

            MoveSelectedTasksCommand = new RelayCommand<TaskListItemViewModel>(async (selectedTaskList) =>
            {
                if (selectedTaskList == null)
                    return;
                await MoveSelectedTasksAsync(selectedTaskList);
            });

            ShowSubTasksCommand = new RelayCommand<TaskItemViewModel>
                ((task) => task.ShowSubTasks = !task.ShowSubTasks);

            SubTaskSelectedItemCommand = new RelayCommand<TaskItemViewModel>
                ((subTask) => OnSubTaskSelected(subTask));
        }

        public async Task GetAllTasksAsync(TaskListItemViewModel taskList)
        {
            UpdateSelectedTasksText(0);
            if (taskList == null)
            {
                OnNoTaskListAvailable();
                return;
            }

            ShowTaskListViewProgressRing = true;

            Tasks.Clear();
            TaskAutoSuggestBoxItems.Clear();

            var dbResponse = await _dataService
                .TaskService
                .GetAsNoTrackingAsync(
                    t => t.TaskList.GoogleTaskListID == taskList.TaskListID &&
                         t.LocalStatus != LocalStatus.DELETED,
                    t => t.OrderBy(ta => ta.Position));

            if (!dbResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An unknown error occurred while trying to retrieve all the tasks from db. Error = {dbResponse.Message}");
                return;
            }

            var tasks = _mapper.Map<List<TaskItemViewModel>>(dbResponse.Result);
            if (tasks.Count > 0)
            {
                var mainTasks = tasks
                    .Where(t => t.ParentTask == null);
                mainTasks.ForEach(t =>
                {
                    if (!tasks.Any(st => st.ParentTask == t.TaskID))
                        return;
                    t.SubTasks = _mapper.Map<ObservableCollection<TaskItemViewModel>>(
                        tasks
                            .Where(st => st.ParentTask == t.TaskID)
                            .OrderBy(st => st.Position));
                });
                Tasks.AddRange(mainTasks);
                TaskAutoSuggestBoxItems
                    .AddRange(_mapper.Map<IEnumerable<ItemModel>>(mainTasks.OrderBy(t => t.Title)));

                SortTasks(_appSettings.DefaultTaskSortOrder);
            }

            CurrentTaskList = taskList;

            //If we have something in the init details, lets select that task
            if (InitDetails is null == false &&
                !string.IsNullOrEmpty(InitDetails.Item1) &&
                !string.IsNullOrEmpty(InitDetails.Item2))
            {
                var selectedTask = Tasks.FirstOrDefault(t => t.TaskID == InitDetails.Item2);
                if (selectedTask is null == false)
                    selectedTask.IsSelected = true;
                InitDetails = null;
            }

            ShowTaskListViewProgressRing = false;
        }

        private void OnNoTaskListAvailable()
        {
            HideControls(false);
            DisableControls(false);
            CanAddMoreTaskList = true;
            CurrentTaskList = null;
            Tasks?.Clear();
            TaskAutoSuggestBoxItems?.Clear();
            //this needs to be clear one i complete the autosuggestbox
            //SelectedTasks.Clear();
            _messenger.Send(false, $"{MessageType.OPEN_PANE}");
        }

        public void OnTaskSelectectionChanged(TaskItemViewModel task)
        {
            int selectedTasks = GetSelectedTasks().Count;
            UpdateSelectedTasksText(selectedTasks);

            if (_isSelectionInProgress)
                return;

            //When you delete/mark as completed multiple tasks, at some point 
            //the selectedTasks count = 0, so lets close the panel
            if (task == null && selectedTasks == 0)
            {
                _messenger.Send(false, $"{MessageType.OPEN_PANE}");
                return;
            }

            //When a list contains no items, and you switch to that list
            //or when you change the selected items, the event raises.
            if (task == null || selectedTasks > 1)
                return;

            var selectedSubTask = GetSelectedSubTask();
            if (selectedSubTask.Any())
                MarkAsSelectedAllSubTasks(false);

            _messenger.Send(task.IsSelected, $"{MessageType.OPEN_PANE}");
            if (task.IsSelected)
                _messenger.Send(task, $"{MessageType.NEW_TASK}");
        }

        public void OnSubTaskSelected(TaskItemViewModel subTask)
        {
            if (subTask == null)
                return;

            MarkAsSelectedAllSubTasks(false, subTask.TaskID);
            MarkAsSelectedAllTasks(false);

            _messenger.Send(subTask.IsSelected, $"{MessageType.OPEN_PANE}");
            if (subTask.IsSelected)
                _messenger.Send(subTask, $"{MessageType.NEW_TASK}");
        }

        public void OnTaskAutoSuggestBoxTextChangeAsync(string currentText)
        {
            //TODO: Refactor this
            var filteredItems = string.IsNullOrEmpty(currentText)
                ? _mapper.Map<IEnumerable<ItemModel>>(Tasks
                    .OrderBy(t => t.Title)
                    .Take(10))
                : _mapper.Map<IEnumerable<ItemModel>>(Tasks
                    .Where(t => t.Title.ToLowerInvariant().Contains(currentText.ToLowerInvariant()))
                    .OrderBy(t => t.Title)
                    .Take(10));
            TaskAutoSuggestBoxItems.Clear();
            TaskAutoSuggestBoxItems.AddRange(filteredItems);
        }

        public void OnTaskAutoSuggestBoxQuerySubmitted(ItemModel selectedItem)
        {
            if (selectedItem == null)
                return;

            MarkAsSelectedAllTasks(false);

            var task = Tasks.FirstOrDefault(t => t.TaskID == selectedItem.ItemId);
            task.IsSelected = true;
            TaskAutoSuggestBoxText = string.Empty;
        }

        public async Task OnTaskSavedAsync(string taskID)
        {
            ShowTaskListViewProgressRing = true;
            var dbResponse = await _dataService
                .TaskService
                .FirstOrDefaultAsNoTrackingAsync(ta => ta.GoogleTaskID == taskID);

            if (!dbResponse.Succeed || dbResponse.Result == null)
            {
                ShowTaskListViewProgressRing = false;
                string msg = dbResponse.Result == null
                    ? "Could not find the saved task in db"
                    : $"An unknown error occurred. Error = {dbResponse.Message}";
                await _dialogService.ShowMessageDialogAsync("Error", msg);
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
                    ShowTaskListViewProgressRing = false;
                    string msg = $"An unknown error occurred. Error = {dbResponse.Message}";
                    await _dialogService.ShowMessageDialogAsync("Error", msg);
                    return;
                }

                task.SubTasks = _mapper.Map<ObservableCollection<TaskItemViewModel>>(stsResponse.Result);
            }

            ShowTaskListViewProgressRing = false;

            if (task.HasParentTask)
            {
                task.IsSelected = true;
                var parentTask = Tasks?
                    .FirstOrDefault(t => t.TaskID == task.ParentTask);

                if (parentTask == null)
                    return;

                int updatedSubTaskIndex = parentTask
                                              .SubTasks?
                                              .ToList()?
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

        public void OnTaskDeleted(string taskID)
        {
            //TODO: Fix this or rethink it
            var taskToDelete = Tasks.FirstOrDefault(t => t.TaskID == taskID);
            if (taskToDelete != null)
                Tasks.Remove(taskToDelete);
        }

        public void OnSubTaskDeleted(string taskID, string subTaskID)
        {
            Tasks
                .FirstOrDefault(t => t.TaskID == taskID)?
                .SubTasks?
                .RemoveAll(st => st.TaskID == subTaskID);
        }

        public void OnTaskStatusChanged(TaskItemViewModel task, bool isSubTask)
        {
            TaskItemViewModel taskFound;
            if (!isSubTask)
                taskFound = Tasks
                    .FirstOrDefault(t => t.TaskID == task.TaskID);
            else
            {
                taskFound = Tasks
                    .FirstOrDefault(t => t.TaskID == task.ParentTask)?
                    .SubTasks?
                    .FirstOrDefault(st => st.TaskID == task.TaskID);
            }

            if (taskFound == null)
                return;

            taskFound.CompletedOn = task.CompletedOn;
            taskFound.UpdatedAt = task.UpdatedAt;
            taskFound.Status = task.Status;
        }

        public void NewTask()
        {
            _isSelectionInProgress = true;
            MarkAsSelectedAllTasks(false);
            var task = new TaskItemViewModel
            {
                IsSelected = true
            };
            _isSelectionInProgress = false;
            OnTaskSelectectionChanged(task);
        }

        public async Task SaveNewTaskListAsync()
        {
            string taskListName = await _dialogService
                .ShowInputStringDialogAsync("Type the task list name", string.Empty, "Save", "Cancel");

            if (string.IsNullOrEmpty(taskListName))
                return;

            ShowTaskListViewProgressRing = true;

            var entity = new GoogleTaskList
            {
                GoogleTaskListID = Guid.NewGuid().ToString(),
                Title = taskListName,
                UpdatedAt = DateTimeOffset.UtcNow,
                LocalStatus = LocalStatus.CREATED,
                ToBeSynced = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            var response = await _dataService
                .TaskListService
                .AddAsync(entity);

            ShowTaskListViewProgressRing = false;

            _messenger.Send(
                _mapper.Map<TaskListItemViewModel>(entity),
                $"{MessageType.TASK_LIST_ADDED}");

            string message = string.Empty;
            if (!response.Succeed)
                message = $"Coudln't create the task list. Error = {response.Message}";
            else
                message = "Task list succesfully created.";

            InAppNotificationRequest?.Invoke(message);
        }

        public async Task ChangeTaskStatusAsync(TaskItemViewModel task, GoogleTaskStatus taskStatus, bool isSubTask)
        {
            string statusMessage =
                $"{(taskStatus == GoogleTaskStatus.COMPLETED ? "completed" : "incompleted")}";

            bool changeStatus = await _dialogService.ShowConfirmationDialogAsync(
                "Confirmation",
                $"Are you sure you want to mark {task.Title} as {statusMessage}?",
                "Yes",
                "No");
            if (!changeStatus)
                return;

            ShowTaskListViewProgressRing = true;

            var response = await _dataService
                .TaskService
                .ChangeTaskStatusAsync(task.TaskID, taskStatus);

            if (!response.Succeed)
            {
                ShowTaskListViewProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to mark the task as {statusMessage}.",
                    $"Error: {response.Message}.");
                return;
            }

            var t = _mapper.Map<TaskItemViewModel>(response.Result);
            OnTaskStatusChanged(t, isSubTask);

            _messenger.Send(
                new Tuple<TaskItemViewModel, bool>(t, isSubTask),
                $"{MessageType.TASK_STATUS_CHANGED_FROM_CONTENT_FRAME}");

            //Only update the subtasks if the new task status is completed
            if (!isSubTask && task.HasSubTasks && taskStatus == GoogleTaskStatus.COMPLETED)
                await ChangeSubTasksStatusAsync(task.SubTasks, taskStatus);

            ShowTaskListViewProgressRing = false;

            InAppNotificationRequest?.Invoke($"{task.Title} was marked as {statusMessage}.");
        }

        public async Task ChangeSelectedTasksStatusAsync(GoogleTaskStatus taskStatus)
        {
            var selectedTasks = GetSelectedTasks()
                .Where(t => t.CanBeMarkedAsCompleted);
            int numberOfSelectedTasks = selectedTasks.Count();

            if (numberOfSelectedTasks == 0)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    "You need to select at least one task that is not already completed");
                return;
            }

            string statusMessage =
                $"{(taskStatus == GoogleTaskStatus.COMPLETED ? "completed" : "incompleted")}";

            bool changeStatus = await _dialogService.ShowConfirmationDialogAsync(
                "Confirmation",
                $"Are you sure you want to mark {numberOfSelectedTasks} task(s) as {statusMessage}?",
                "Yes",
                "No");
            if (!changeStatus)
                return;

            ShowTaskListViewProgressRing = true;

            var tasksStatusNotChanged = new List<string>();
            //TODO: THIS COULD BE IMPROVED
            foreach (var task in selectedTasks)
            {
                var response = await _dataService
                    .TaskService
                    .ChangeTaskStatusAsync(task.TaskID, taskStatus);

                if (!response.Succeed)
                {
                    tasksStatusNotChanged.Add(task.Title);
                    continue;
                }

                var t = _mapper.Map<TaskItemViewModel>(response.Result);
                OnTaskStatusChanged(t, false);
                task.IsSelected = false;

                _messenger.Send(
                    new Tuple<TaskItemViewModel, bool>(t, false),
                    $"{MessageType.TASK_STATUS_CHANGED_FROM_CONTENT_FRAME}");

                if (task.HasSubTasks)
                    await ChangeSubTasksStatusAsync(task.SubTasks, taskStatus);
            }

            ShowTaskListViewProgressRing = false;

            if (tasksStatusNotChanged.Count > 0)
            {
                await _dialogService.ShowMessageDialogAsync(
                    $"Error",
                    $"An error occurred, coudln't mark as {statusMessage} the following tasks: {string.Join(",", tasksStatusNotChanged)}");
            }
        }

        private async Task ChangeSubTasksStatusAsync(IEnumerable<TaskItemViewModel> tasks, GoogleTaskStatus taskStatus)
        {
            string statusMessage =
                $"{(taskStatus == GoogleTaskStatus.COMPLETED ? "completed" : "incompleted")}";

            var tasksStatusNotChanged = new List<string>();
            foreach (var task in tasks)
            {
                var response = await _dataService
                    .TaskService
                    .ChangeTaskStatusAsync(task.TaskID, taskStatus);

                if (!response.Succeed)
                {
                    tasksStatusNotChanged.Add(task.Title);
                    continue;
                }

                var t = _mapper.Map<TaskItemViewModel>(response.Result);
                OnTaskStatusChanged(t, true);

                _messenger.Send(
                    new Tuple<TaskItemViewModel, bool>(t, true),
                    $"{MessageType.TASK_STATUS_CHANGED_FROM_CONTENT_FRAME}");
            }

            if (tasksStatusNotChanged.Count > 0)
            {
                await _dialogService.ShowMessageDialogAsync(
                    $"Error",
                    $"An error occurred, coudln't mark as {statusMessage} the following tasks: {string.Join(",", tasksStatusNotChanged)}");
            }
        }

        public async Task DeleteTaskAsync(TaskItemViewModel task)
        {
            bool deleteTask = await _dialogService.ShowConfirmationDialogAsync(
                "Confirmation",
                "Are you sure you wanna delete this task?",
                "Yes",
                "No");

            if (!deleteTask)
                return;
            ShowTaskListViewProgressRing = true;
            _messenger.Send(true, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");

            var deleteResponse = await _dataService
                .TaskService
                .RemoveTaskAsync(task.TaskID);

            if (TasksHelper.HasReminderId(task.RemindOnGUID, out int id))
            {
                _notificationService.RemoveScheduledNotification(id);
            }

            if (task.HasSubTasks)
            {
                foreach (var st in task.SubTasks)
                    if (TasksHelper.HasReminderId(st.RemindOnGUID, out int stReminderId))
                        _notificationService.RemoveScheduledNotification(stReminderId);
            }


            ShowTaskListViewProgressRing = false;
            _messenger.Send(false, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");

            if (!deleteResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the selected task. Error = {deleteResponse.Message}");
            }
            else
            {
                _isSelectionInProgress = true;
                Tasks.RemoveAll(t => t.TaskID == task.TaskID);
                _isSelectionInProgress = false;
                _messenger.Send(task.TaskID, $"{MessageType.TASK_DELETED_FROM_CONTENT_FRAME}");
            }
        }

        public async Task DeleteSelectedTasksAsync()
        {
            var tasksToDelete = GetSelectedTasks();
            int numberOfTasksToDelete = tasksToDelete.Count();

            if (numberOfTasksToDelete == 0)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    "You need to select at least one task");
                return;
            }

            bool deleteSelectedTasks = await _dialogService.ShowConfirmationDialogAsync(
                "Confirmation",
                $"Are you sure you wanna delete {numberOfTasksToDelete} task(s)?",
                "Yes",
                "No");

            if (!deleteSelectedTasks)
                return;

            _messenger.Send(true, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");
            ShowTaskListViewProgressRing = true;

            var deleteResponse = await _dataService
                .TaskService
                .RemoveTaskAsync(tasksToDelete.Select(t => t.TaskID));

            foreach (var task in tasksToDelete)
            {
                if (TasksHelper.HasReminderId(task.RemindOnGUID, out int id))
                    _notificationService.RemoveScheduledNotification(id);

                if (task.HasSubTasks)
                {
                    foreach (var st in task.SubTasks)
                        if (TasksHelper.HasReminderId(st.RemindOnGUID, out int stReminderId))
                            _notificationService.RemoveScheduledNotification(stReminderId);
                }
            }

            ShowTaskListViewProgressRing = false;
            _messenger.Send(false, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");

            if (!deleteResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An error occurred, coudln't delete the selected tasks. Error = {deleteResponse.Message}");
                return;
            }

            _messenger.Send(
                string.Join(",", tasksToDelete.Select(t => t.TaskID)),
                $"{MessageType.TASK_DELETED_FROM_CONTENT_FRAME}");

            _isSelectionInProgress = true;
            Tasks.RemoveAll(t => tasksToDelete.Any(tr => t.TaskID == tr.TaskID));
            _isSelectionInProgress = false;
        }

        public async Task MoveTaskAsync(TaskItemViewModel selectedTask, TaskListItemViewModel selectedTaskList)
        {
            bool move = await _dialogService.ShowConfirmationDialogAsync(
                "Confirm",
                $"Are you sure you want to move {selectedTask.Title} to {selectedTaskList.Title}",
                "Yes",
                "No");
            if (!move)
                return;

            ShowTaskListViewProgressRing = true;

            var dbResponse = await _dataService
                .TaskService
                .MoveAsync(selectedTaskList.TaskListID, selectedTask.TaskID, selectedTask.ParentTask,
                    selectedTask.Position);

            if (!dbResponse.Succeed)
            {
                ShowTaskListViewProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An error occurred while trying to move the selected task from {_currentTaskList.Title} to {selectedTaskList.Title}. " +
                    $"Error = {dbResponse.Message}.");
                return;
            }

            if (TasksHelper.HasReminderId(selectedTask.RemindOnGUID, out int id))
                ReAddReminderDate(id, selectedTaskList, dbResponse.Result);

            if (selectedTask.HasSubTasks)
            {
                selectedTask.SubTasks.ForEach(st => st.ParentTask = dbResponse.Result.GoogleTaskID);
                var subTaskMoved = await MoveSubTasksAsync(selectedTaskList.TaskListID, selectedTask.SubTasks);
                foreach (var st in subTaskMoved)
                    if (TasksHelper.HasReminderId(st.RemindOnGUID, out int stNotifId))
                        ReAddReminderDate(stNotifId, selectedTaskList, st);
            }

            ShowTaskListViewProgressRing = false;
            SelectedTaskToMove = null;

            _isSelectionInProgress = true;
            Tasks.Remove(selectedTask);
            _isSelectionInProgress = false;

            _messenger.Send(selectedTask.TaskID, $"{MessageType.TASK_DELETED_FROM_CONTENT_FRAME}");

            InAppNotificationRequest?
                .Invoke($"Task sucessfully moved from: {_currentTaskList.Title} to: {selectedTaskList.Title}");
        }

        public async Task MoveSelectedTasksAsync(TaskListItemViewModel selectedTaskList)
        {
            var selectedTasks = GetSelectedTasks();
            if (selectedTaskList == null || selectedTasks.Count() == 0)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    "You must select at least one task");
                return;
            }

            bool move = await _dialogService.ShowConfirmationDialogAsync(
                "Confirm",
                $"Are you sure you want to move {selectedTasks.Count()} task(s) to {selectedTaskList.Title}",
                "Yes",
                "No");
            if (!move)
                return;

            ShowTaskListViewProgressRing = true;

            var tasksNotMoved = new List<string>();
            var tasksMoved = new List<string>();

            foreach (var task in selectedTasks)
            {
                var moveResponse = await _dataService
                    .TaskService
                    .MoveAsync(selectedTaskList.TaskListID, task.TaskID, task.ParentTask, task.Position);
                if (!moveResponse.Succeed)
                    tasksNotMoved.Add(task.Title);
                else
                {
                    if (TasksHelper.HasReminderId(task.RemindOnGUID, out int id))
                        ReAddReminderDate(id, selectedTaskList, moveResponse.Result);

                    tasksMoved.Add(task.TaskID);
                    if (task.HasSubTasks)
                    {
                        task.SubTasks.ForEach(s => s.ParentTask = moveResponse.Result.GoogleTaskID);
                        var subTaskMoved = await MoveSubTasksAsync(selectedTaskList.TaskListID, task.SubTasks);
                        foreach (var st in subTaskMoved)
                            if (TasksHelper.HasReminderId(st.RemindOnGUID, out int stNotifId))
                                ReAddReminderDate(stNotifId, selectedTaskList, st);
                    }
                }
            }


            ShowTaskListViewProgressRing = false;
            SelectedTaskToMove = null;

            if (tasksMoved.Count > 0)
                _messenger.Send(
                    string.Join(",", tasksMoved),
                    $"{MessageType.TASK_DELETED_FROM_CONTENT_FRAME}");

            _isSelectionInProgress = true;
            Tasks.RemoveAll(t => tasksMoved.Any(tr => t.TaskID == tr));
            _isSelectionInProgress = false;

            if (tasksNotMoved.Count > 0)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An error occurred, coudln't move this tasks: {string.Join(",", tasksNotMoved)}");
                return;
            }

            InAppNotificationRequest?
                .Invoke($"Moved {selectedTasks.Count()} task(s) to {selectedTaskList.Title}");
        }

        public async Task<List<GoogleTask>> MoveSubTasksAsync(string taskListID, IEnumerable<TaskItemViewModel> subTasks)
        {
            var stList = new List<GoogleTask>();
            foreach (var st in subTasks)
            {
                var moveResponse = await _dataService
                    .TaskService
                    .MoveAsync(taskListID, st.TaskID, st.ParentTask, stList.LastOrDefault()?.GoogleTaskID);
                if (moveResponse.Succeed)
                    stList.Add(moveResponse.Result);
            }

            return stList;
        }

        public void SortTasks(TaskSortType sortType)
        {
            //TODO: WHEN YOU SORT, THE SELECTED ITEM GETS LOST
            if (Tasks == null)
                return;

            _isSelectionInProgress = true;
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
            _isSelectionInProgress = false;
        }

        private void DisableControls(bool isEnabled)
        {
            IsAutoSuggestBoxEnabled = isEnabled;
            CanAddMoreTaskList = isEnabled;
            CanAddMoreTasks = isEnabled;
            CanDeleteTasks = isEnabled;
            CanRefreshTaskListView = isEnabled;
            CanSortTaskListView = isEnabled;
        }

        private void HideControls(bool isVisible)
        {
            IsTaskListTitleVisible = isVisible;
            IsTaskListViewVisible = isVisible;
        }

        private List<TaskItemViewModel> GetSelectedTasks()
            => Tasks?.Where(t => t.IsSelected).ToList() ?? Enumerable.Empty<TaskItemViewModel>().ToList();

        private IEnumerable<TaskItemViewModel> GetSelectedSubTask()
        {
            return Tasks
                .Where(t => t.SubTasks != null)
                .SelectMany(t => t.SubTasks.Where(st => st.IsSelected));
        }

        private void MarkAsSelectedAllTasks(bool isSelected, string exceptTaskID = null)
        {
            if (!string.IsNullOrEmpty(exceptTaskID))
            {
                Tasks?
                    .Where(t => t.TaskID != exceptTaskID && t.IsSelected != isSelected)
                    .ForEach(t => t.IsSelected = isSelected);
            }
            else
                Tasks?
                    .Where(t => t.IsSelected != isSelected)
                    .ForEach(t => t.IsSelected = isSelected);
        }

        private void MarkAsSelectedAllSubTasks(bool isSelected, string exceptSubTaskID = null)
        {
            if (!string.IsNullOrEmpty(exceptSubTaskID))
            {
                Tasks?.ForEach(t => t.SubTasks?
                    .Where(st => st.TaskID != exceptSubTaskID && st.IsSelected != isSelected)
                    .ForEach(st => st.IsSelected = isSelected));
            }
            else
            {
                Tasks?
                    .ForEach(t =>
                        t.SubTasks?
                            .Where(st => st.IsSelected != isSelected)
                            .ForEach(st => st.IsSelected = isSelected));
            }
        }

        private void UpdateSelectedTasksText(int selectedTasks)
        {
            SelectedTasksText = selectedTasks > 0
                ? $"You have selected {selectedTasks} task(s)"
                : string.Empty;
        }

        private void ReAddReminderDate(
            int notificationId,
            TaskListItemViewModel taskList,
            GoogleTask task)
        {
            string notes = TasksHelper.GetNotesForNotification(task.Notes);
            _notificationService.RemoveScheduledNotification(notificationId);
            _notificationService.ScheduleNotification(new TaskReminderNotification
            {
                Id = notificationId,
                TaskListId = taskList.TaskListID,
                TaskId = task.GoogleTaskID,
                TaskListTitle = taskList.Title,
                TaskTitle = task.Title,
                TaskBody = notes,
                DeliveryOn = task.RemindOn.Value
            });
        }
        #endregion
    }
}