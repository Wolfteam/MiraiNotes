using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.Data.Models;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.Shared.Helpers;
using MiraiNotes.Shared.Models;
using MiraiNotes.UWP.Extensions;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Utils;
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
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IMapper _mapper;
        private readonly IMiraiNotesDataService _dataService;
        private readonly ISyncService _syncService;

        private TaskListItemViewModel _currentTaskList;

        private SmartObservableCollection<TaskItemViewModel> _tasks = new SmartObservableCollection<TaskItemViewModel>();
        private SmartObservableCollection<ItemModel> _taskAutoSuggestBoxItems = new SmartObservableCollection<ItemModel>();

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

        private ObservableCollection<TaskListItemViewModel> _taskLists = new ObservableCollection<TaskListItemViewModel>();
        private bool _showMoveTaskFlyoutProgressBar;
        #endregion

        #region Properties
        public SmartObservableCollection<TaskItemViewModel> Tasks
        {
            get { return _tasks; }
            set { _tasks = value; }
        }

        public SmartObservableCollection<ItemModel> TaskAutoSuggestBoxItems
        {
            get { return _taskAutoSuggestBoxItems; }
            set { _taskAutoSuggestBoxItems = value; }
        }

        public TaskListItemViewModel CurrentTaskList
        {
            get { return _currentTaskList; }
            set { SetValue(ref _currentTaskList, value); }
        }

        public bool IsTaskListTitleVisible
        {
            get { return _isTaskListTitleVisible; }
            set { SetValue(ref _isTaskListTitleVisible, value); }
        }

        public bool IsTaskListViewVisible
        {
            get { return _isTaskListViewVisible; }
            set { SetValue(ref _isTaskListViewVisible, value); }
        }

        public bool IsAutoSuggestBoxEnabled
        {
            get { return _isAutoSuggestBoxEnabled; }
            set { SetValue(ref _isAutoSuggestBoxEnabled, value); }
        }

        public bool CanAddMoreTaskList
        {
            get { return _canAddMoreTaskList; }
            set { SetValue(ref _canAddMoreTaskList, value); }
        }

        public bool CanAddMoreTasks
        {
            get { return _canAddMoreTasks; }
            set { SetValue(ref _canAddMoreTasks, value); }
        }

        public bool CanDeleteTasks
        {
            get { return _canDeleteTasks; }
            set { SetValue(ref _canDeleteTasks, value); }
        }

        public bool CanRefreshTaskListView
        {
            get { return _canRefreshTaskListView; }
            set { SetValue(ref _canRefreshTaskListView, value); }
        }

        public bool CanSortTaskListView
        {
            get { return _canSortTaskListView; }
            set { SetValue(ref _canSortTaskListView, value); }
        }

        public bool IsTaskListCommandBarCompact
        {
            get { return _isTaskListCommandBarCompact; }
            set { SetValue(ref _isTaskListCommandBarCompact, value); }
        }

        public bool ShowTaskListViewProgressRing
        {
            get { return _showTaskListViewProgressRing; }
            set
            {
                HideControls(!value);
                DisableControls(!value);
                SetValue(ref _showTaskListViewProgressRing, value);
            }
        }

        public string SelectedTasksText
        {
            get { return _selectedTasksText; }
            set { SetValue(ref _selectedTasksText, value); }
        }

        public string TaskAutoSuggestBoxText
        {
            get { return _taskAutoSuggestBoxText; }
            set { SetValue(ref _taskAutoSuggestBoxText, value); }
        }


        public ObservableCollection<TaskListItemViewModel> TaskLists
        {
            get { return _taskLists; }
            set { SetValue(ref _taskLists, value); }
        }

        public TaskItemViewModel SelectedTaskToMove { get; set; }

        public bool ShowMoveTaskFlyoutProgressBar
        {
            get { return _showMoveTaskFlyoutProgressBar; }
            set { SetValue(ref _showMoveTaskFlyoutProgressBar, value); }
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

        #region Constructors
        public TasksPageViewModel(
            ICustomDialogService dialogService,
            IMessenger messenger,
            INavigationService navigationService,
            IUserCredentialService userCredentialService,
            IMapper mapper,
            IMiraiNotesDataService dataService,
            ISyncService syncService)
        {
            _dialogService = dialogService;
            _messenger = messenger;
            _navigationService = navigationService;
            _userCredentialService = userCredentialService;
            _mapper = mapper;
            _dataService = dataService;
            _syncService = syncService;

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

            _messenger.Register<TaskItemViewModel>(this, $"{MessageType.TASK_SAVED}", OnTaskSaved);
            _messenger.Register<string>(this, $"{MessageType.TASK_DELETED_FROM_PANE_FRAME}", OnTaskDeleted);
            _messenger.Register<KeyValuePair<string, string>>(
                this,
                $"{MessageType.SUBTASK_DELETED_FROM_PANE_FRAME}",
                (kvp) => OnSubTaskDeleted(kvp.Key, kvp.Value));
            _messenger.Register<Tuple<TaskItemViewModel, bool>>(
                this,
                $"{MessageType.TASK_STATUS_CHANGED_FROM_PANE_FRAME}",
                (tuple) => OnTaskStatusChanged(tuple.Item1, tuple.Item2));
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

            DeleteTaskCommand = new RelayCommand<string>
                (async (taskID) => await DeleteTaskAsync(taskID));

            DeleteSelectedTasksCommand = new RelayCommand
                (async () => await DeleteSelectedTasksAsync());

            MarkAsCompletedCommand = new RelayCommand<TaskItemViewModel>
                (async (task) => await ChangeTaskStatusAsync(task, GoogleTaskStatus.COMPLETED, task.HasParentTask));

            MarkAsIncompletedCommand = new RelayCommand<TaskItemViewModel>
                (async (task) => await ChangeTaskStatusAsync(task, GoogleTaskStatus.NEEDS_ACTION, task.HasParentTask));

            MarkAsCompletedSelectedTasksCommand = new RelayCommand
                (async () => await ChangeSelectedTasksStatusAsync(GoogleTaskStatus.COMPLETED));

            SyncCommand = new RelayCommand
                (async () =>
                {
                    await Sync();
                    _messenger.Send(true, $"{MessageType.ON_FULL_SYNC}");
                });

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

        private async Task Sync()
        {
            _messenger.Send(false, $"{MessageType.OPEN_PANE}");
            ShowTaskListViewProgressRing = true;

            var syncResults = new List<EmptyResponse>
            {
                await _syncService.SyncDownTaskListsAsync(false),
                await _syncService.SyncDownTasksAsync(false),
                await _syncService.SyncUpTaskListsAsync(false),
                await _syncService.SyncUpTasksAsync(false)
            };

            string message = syncResults.Any(r => !r.Succeed) ?
                string.Join(",\n", syncResults.Where(r => !r.Succeed).Select(r => r.Message).Distinct()) :
                "A full sync was successfully performed.";

            if (string.IsNullOrEmpty(message))
                message = "An unknown error occurred while trying to perform the sync operation.";

            ShowTaskListViewProgressRing = false;

            if (syncResults.Any(r => !r.Succeed))
                await _dialogService
                    .ShowMessageDialogAsync("Error", $"A full sync completed with errors: {message}");
            else
                await _dialogService.ShowMessageDialogAsync("Completed", $"{message}");
        }

        public async Task GetAllTasksAsync(TaskListItemViewModel taskList)
        {
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
            }
            CurrentTaskList = taskList;
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
            int selectedTasks = GetSelectedTasks()
                .Count();
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
            if (selectedSubTask.Count() > 0)
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
            var filteredItems = string.IsNullOrEmpty(currentText) ?
                _mapper.Map<IEnumerable<ItemModel>>(Tasks
                    .OrderBy(t => t.Title)
                    .Take(10)) :
                _mapper.Map<IEnumerable<ItemModel>>(Tasks
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

            var task = Tasks.FirstOrDefault(t => t.TaskID == selectedItem.ItemID);
            task.IsSelected = true;
            TaskAutoSuggestBoxText = string.Empty;
        }

        public void OnTaskSaved(TaskItemViewModel task)
        {
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
                {
                    parentTask.SubTasks[updatedSubTaskIndex] = task;
                }
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
                //TODO: I should show a different list for completed tasks
                //if (task.TaskStatus == GoogleTaskStatus.NEEDS_ACTION)
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
            TaskItemViewModel taskFound = null;
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
                UpdatedAt = DateTime.Now,
                LocalStatus = LocalStatus.CREATED,
                ToBeSynced = true,
                CreatedAt = DateTime.Now
            };
            var response = await _dataService
                .TaskListService
                .AddAsync(entity);

            ShowTaskListViewProgressRing = false;
            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't create the task list. Error = {response.Message}");
                return;
            }
            await _dialogService.ShowMessageDialogAsync("Succeed", "Task list created.");
            _messenger.Send(
                _mapper.Map<TaskListItemViewModel>(entity),
                $"{MessageType.TASK_LIST_ADDED}");
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

            var dbResponse = await _dataService
                .TaskService
                .FirstOrDefaultAsNoTrackingAsync(tx => tx.GoogleTaskID == task.TaskID);

            if (!dbResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An unknown error occurred while trying to retrieve task from db. Error = {dbResponse.Message}");
                return;
            }

            if (taskStatus == GoogleTaskStatus.COMPLETED)
                dbResponse.Result.CompletedOn = DateTime.Now;
            else
                dbResponse.Result.CompletedOn = null;
            dbResponse.Result.Status = taskStatus.GetString();
            dbResponse.Result.UpdatedAt = DateTime.Now;
            if (dbResponse.Result.LocalStatus != LocalStatus.CREATED)
                dbResponse.Result.LocalStatus = LocalStatus.UPDATED;
            dbResponse.Result.ToBeSynced = true;

            var response = await _dataService
                .TaskService
                .UpdateAsync(dbResponse.Result);

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

            await _dialogService.ShowMessageDialogAsync(
                "Succeed",
                $"{task.Title} was marked as {statusMessage}.");
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
            var dbResponse = await _dataService
                .TaskService
                .GetAsync(t => selectedTasks.Any(st => st.TaskID == t.GoogleTaskID));

            if (!dbResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An unknown error occurred while trying to retrieve the selected tasks from db. Error = {dbResponse.Message}");
                return;
            }

            foreach (var task in selectedTasks)
            {
                var taskToUpdateDbResponse = await _dataService
                    .TaskService
                    .FirstOrDefaultAsNoTrackingAsync(ta => ta.GoogleTaskID == task.TaskID);

                if (!taskToUpdateDbResponse.Succeed)
                {
                    tasksStatusNotChanged.Add(task.Title);
                    continue;
                }
                taskToUpdateDbResponse.Result.CompletedOn = taskStatus == GoogleTaskStatus.COMPLETED ?
                    DateTime.Now : (DateTime?)null;
                taskToUpdateDbResponse.Result.Status = taskStatus.GetString();
                taskToUpdateDbResponse.Result.UpdatedAt = DateTime.Now;
                if (taskToUpdateDbResponse.Result.LocalStatus != LocalStatus.CREATED)
                    taskToUpdateDbResponse.Result.LocalStatus = LocalStatus.UPDATED;
                taskToUpdateDbResponse.Result.ToBeSynced = true;

                var updateResponse = await _dataService
                    .TaskService
                    .UpdateAsync(taskToUpdateDbResponse.Result);

                if (!updateResponse.Succeed)
                {
                    tasksStatusNotChanged.Add(task.Title);
                    continue;
                }
                var t = _mapper.Map<TaskItemViewModel>(updateResponse.Result);
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
                var taskToUpdateResponse = await _dataService
                    .TaskService
                    .FirstOrDefaultAsNoTrackingAsync(ta => ta.GoogleTaskID == task.TaskID);

                if (!taskToUpdateResponse.Succeed)
                {
                    tasksStatusNotChanged.Add(task.Title);
                    continue;
                }
                //TODO: I SHOULD MOVE ALL THIS CHANGE STATUS LOGIC TO THE DATA SERVICE
                taskToUpdateResponse.Result.CompletedOn = taskStatus == GoogleTaskStatus.COMPLETED ?
                    DateTime.Now : (DateTime?)null;
                taskToUpdateResponse.Result.Status = taskStatus.GetString();
                taskToUpdateResponse.Result.UpdatedAt = DateTime.Now;
                if (taskToUpdateResponse.Result.LocalStatus != LocalStatus.CREATED)
                    taskToUpdateResponse.Result.LocalStatus = LocalStatus.UPDATED;
                taskToUpdateResponse.Result.ToBeSynced = true;

                var updateResponse = await _dataService
                    .TaskService
                    .UpdateAsync(taskToUpdateResponse.Result);

                if (!updateResponse.Succeed)
                {
                    tasksStatusNotChanged.Add(task.Title);
                    continue;
                }

                var t = _mapper.Map<TaskItemViewModel>(updateResponse.Result);
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

        public async Task DeleteTaskAsync(string taskID)
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

            var dbResponse = await _dataService
                .TaskService
                .FirstOrDefaultAsNoTrackingAsync(t => t.GoogleTaskID == taskID);

            if (!dbResponse.Succeed)
            {
                ShowTaskListViewProgressRing = false;
                _messenger.Send(false, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An unknown error occurred while trying to retrieve task from db. Error = {dbResponse.Message}");
                return;
            }

            EmptyResponse deleteResponse;
            if (dbResponse.Result.LocalStatus == LocalStatus.CREATED)
            {
                deleteResponse = await _dataService
                   .TaskService
                   .RemoveAsync(dbResponse.Result);
            }
            else
            {
                dbResponse.Result.LocalStatus = LocalStatus.DELETED;
                dbResponse.Result.UpdatedAt = DateTime.Now;
                dbResponse.Result.ToBeSynced = true;

                deleteResponse = await _dataService
                    .TaskService
                    .UpdateAsync(dbResponse.Result);
            }

            ShowTaskListViewProgressRing = false;
            _messenger.Send(false, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");

            if (!deleteResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the selected task. Error code = {deleteResponse.Message}");
            }
            else
            {
                _isSelectionInProgress = true;
                Tasks.RemoveAll(t => t.TaskID == taskID);
                _isSelectionInProgress = false;
                _messenger.Send(taskID, $"{MessageType.TASK_DELETED_FROM_CONTENT_FRAME}");
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

            var dbResponse = await _dataService
                .TaskService
                .GetAsNoTrackingAsync(
                    t => tasksToDelete.Any(td => t.GoogleTaskID == td.TaskID),
                    null,
                    string.Empty);

            if (!dbResponse.Succeed)
            {
                ShowTaskListViewProgressRing = false;
                _messenger.Send(false, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An unknown error occurred while trying to retrieve the tasks from db. Error = {dbResponse.Message}");
                return;
            }

            var deleteResponse = new List<EmptyResponse>();

            if (dbResponse.Result.Any(t => t.LocalStatus == LocalStatus.CREATED))
            {
                deleteResponse.Add(await _dataService
                    .TaskService
                    .RemoveRangeAsync(dbResponse.Result.Where(t => t.LocalStatus == LocalStatus.CREATED)));
            }
            if (dbResponse.Result.Any(t => t.LocalStatus != LocalStatus.CREATED))
            {
                dbResponse.Result.Where(t => t.LocalStatus != LocalStatus.CREATED).ForEach(t =>
                {
                    t.UpdatedAt = DateTime.Now;
                    t.LocalStatus = LocalStatus.DELETED;
                    t.ToBeSynced = true;
                });
                deleteResponse.Add(await _dataService
                    .TaskService
                    .UpdateRangeAsync(dbResponse.Result.Where(t => !t.ToBeSynced)));
            }

            ShowTaskListViewProgressRing = false;
            _messenger.Send(false, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");

            if (deleteResponse.Any(r => !r.Succeed))
            {
                string message = string.Join(",", deleteResponse.Where(r => !r.Succeed).Select(r => r.Message));
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An error occurred, coudln't delete the selected tasks. Error = {message}");
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
                .MoveAsync(selectedTaskList.TaskListID, selectedTask.TaskID, selectedTask.ParentTask, selectedTask.Position);

            if (!dbResponse.Succeed)
            {
                ShowTaskListViewProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An error occurred while trying to move the selected task from {_currentTaskList.Title} to {selectedTaskList.Title}. " +
                    $"Error = {dbResponse.Message}.");
                return;
            }

            if (selectedTask.HasSubTasks)
            {
                selectedTask.SubTasks.ForEach(st => st.ParentTask = dbResponse.Result.GoogleTaskID);
                await MoveSubTasksAsync(selectedTaskList.TaskListID, selectedTask.SubTasks);
            }
            ShowTaskListViewProgressRing = false;
            SelectedTaskToMove = null;

            _isSelectionInProgress = true;
            Tasks.Remove(selectedTask);
            _isSelectionInProgress = false;

            _messenger.Send(selectedTask.TaskID, $"{MessageType.TASK_DELETED_FROM_CONTENT_FRAME}");

            await _dialogService.ShowMessageDialogAsync(
                "Succeed",
                $"Task sucessfully moved from: {_currentTaskList.Title} to: {selectedTaskList.Title}");
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
                    tasksMoved.Add(task.TaskID);
                    if (task.HasSubTasks)
                    {
                        task.SubTasks.ForEach(s => s.ParentTask = moveResponse.Result.GoogleTaskID);
                        await MoveSubTasksAsync(selectedTaskList.TaskListID, task.SubTasks);
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

            await _dialogService.ShowMessageDialogAsync(
                "Succeed",
                $"Moved {selectedTasks.Count()} task(s) to {selectedTaskList.Title}");
        }

        public async Task MoveSubTasksAsync(string taskListID, IEnumerable<TaskItemViewModel> subTasks)
        {
            var stList = new List<string>();
            foreach (var st in subTasks)
            {
                var moveResponse = await _dataService
                    .TaskService
                    .MoveAsync(taskListID, st.TaskID, st.ParentTask, stList.LastOrDefault());
                if (moveResponse.Succeed)
                    stList.Add(moveResponse.Result.GoogleTaskID);
            }
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
                    throw new ArgumentOutOfRangeException("The TaskSortType doesnt have a default sort type");
            }
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

        private IEnumerable<TaskItemViewModel> GetSelectedTasks()
            => Tasks?.Where(t => t.IsSelected) ?? Enumerable.Empty<TaskItemViewModel>();

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
            if (selectedTasks > 0)
                SelectedTasksText = $"You have selected {selectedTasks} task(s)";
            else
                SelectedTasksText = string.Empty;
        }
        #endregion
    }
}