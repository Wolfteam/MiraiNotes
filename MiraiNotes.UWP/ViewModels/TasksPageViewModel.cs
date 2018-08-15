using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.UWP.Extensions;
using MiraiNotes.UWP.Helpers;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Models.API;
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
        private readonly IGoogleApiService _googleApiService;
        private readonly IMapper _mapper;

        private TaskListItemViewModel _currentTaskList;

        private ObservableCollection<TaskItemViewModel> _tasks;
        private ObservableCollection<ItemModel> _taskAutoSuggestBoxItems;

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

        private ObservableCollection<TaskListItemViewModel> _taskLists;
        private bool _showMoveTaskFlyoutProgressBar;
        #endregion

        #region Properties
        public ObservableCollection<TaskItemViewModel> Tasks
        {
            get { return _tasks; }
            set { SetValue(ref _tasks, value); }
        }

        public ObservableCollection<ItemModel> TaskAutoSuggestBoxItems
        {
            get { return _taskAutoSuggestBoxItems; }
            set { SetValue(ref _taskAutoSuggestBoxItems, value); }
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

        public ICommand RefreshTasksCommand { get; set; }

        public ICommand SortTasksCommand { get; set; }

        public ICommand MoveComboBoxClickedCommand { get; set; }

        public ICommand MoveComboBoxSelectionChangedCommand { get; set; }

        public ICommand MoveComboBoxOpenedCommand { get; set; }

        public ICommand MoveSelectedTasksCommand { get; set; }

        public ICommand ShowSubTasksCommand { get; set; }
        #endregion

        #region Constructors
        public TasksPageViewModel(
            ICustomDialogService dialogService,
            IMessenger messenger,
            INavigationService navigationService,
            IUserCredentialService userCredentialService,
            IGoogleApiService googleApiService,
            IMapper mapper)
        {
            _dialogService = dialogService;
            _messenger = messenger;
            _navigationService = navigationService;
            _userCredentialService = userCredentialService;
            _googleApiService = googleApiService;
            _mapper = mapper;

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
            _messenger.Register<string>(this, $"{MessageType.TASK_DELETED}", OnTaskDeleted);
            _messenger.Register<KeyValuePair<string, string>>(
                this,
                $"{MessageType.SUBTASK_DELETED}",
                (kvp) => OnSubTaskDeleted(kvp.Key, kvp.Value));
            _messenger.Register<Tuple<TaskItemViewModel, bool>>(
                this,
                $"{MessageType.TASK_STATUS_CHANGED_FROM_PANE_FRAME}",
                (tuple) => OnTaskStatusChanged(tuple.Item1, tuple.Item2));
        }

        private void SetCommands()
        {
            TaskListViewSelectedItemCommand = new RelayCommand<TaskItemViewModel>
                ((task) => OnTaskListViewSelectedItem(task));

            TaskAutoSuggestBoxTextChangedCommand = new RelayCommand<string>
                (async (text) => await OnTaskAutoSuggestBoxTextChangeAsync(text));

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

            RefreshTasksCommand = new RelayCommand
                (async () => await GetAllTasksAsync(CurrentTaskList));

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
                var response = await _googleApiService
                    .TaskListService
                    .GetAllAsync();
                ShowMoveTaskFlyoutProgressBar = false;

                if (response.Succeed)
                    TaskLists = _mapper.Map<ObservableCollection<TaskListItemViewModel>>
                        (response.Result.Items
                        .Where(t => t.TaskListID != _currentTaskList.TaskListID)
                        .OrderBy(t => t.Title));
                else
                    TaskLists = null;
            });

            MoveSelectedTasksCommand = new RelayCommand<TaskListItemViewModel>(async (selectedTaskList) =>
            {
                if (selectedTaskList == null)
                    return;
                await MoveSelectedTasksAsync(selectedTaskList);
            });

            ShowSubTasksCommand = new RelayCommand<TaskItemViewModel>
                ((task) => task.ShowSubTasks = !task.ShowSubTasks);
        }

        public async Task GetAllTasksAsync(TaskListItemViewModel taskList)
        {
            if (taskList == null)
            {
                OnNoTaskListAvailable();
                return;
            }

            ShowTaskListViewProgressRing = true;
            var response = await _googleApiService.TaskService.GetAllAsync(taskList.TaskListID);
            ShowTaskListViewProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Couldn't get the tasks for the selected task list",
                    $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}");
                return;
            }

            if (response.Result.Items != null && response.Result.Items.Count() > 0)
            {
                var mainTasks = _mapper.Map<ObservableCollection<TaskItemViewModel>>(
                    response.Result.Items
                    .Where(t => t.ParentTask == null)
                    .ToList());

                mainTasks.ForEach(t =>
                {
                    t.SubTasks = _mapper.Map<ObservableCollection<TaskItemViewModel>>(
                        response.Result.Items
                        .Where(st => st.ParentTask == t.TaskID)
                        .OrderBy(st => st.Position));
                });

                Tasks = mainTasks;
                TaskAutoSuggestBoxItems = _mapper.Map<ObservableCollection<ItemModel>>(mainTasks.OrderBy(t => t.Title));
            }
            else
            {
                Tasks = new ObservableCollection<TaskItemViewModel>();
                TaskAutoSuggestBoxItems = new ObservableCollection<ItemModel>();
            }
            CurrentTaskList = taskList;
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

        public void OnTaskListViewSelectedItem(TaskItemViewModel task)
        {
            int selectedTasks = GetSelectedTasks()
                .Count();
            UpdateSelectedTasksText(selectedTasks);

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

            _messenger.Send(task.IsSelected, $"{MessageType.OPEN_PANE}");
            if (task.IsSelected)
                _messenger.Send(task, $"{MessageType.NEW_TASK}");
        }

        public async Task OnTaskAutoSuggestBoxTextChangeAsync(string currentText)
        {
            //TODO: Refactor this
            await Task.Delay(1);
            var filteredItems = string.IsNullOrEmpty(currentText) ?
                _mapper.Map<ObservableCollection<ItemModel>>(Tasks
                    .OrderBy(t => t.Title)
                    .Take(10)) :
                _mapper.Map<ObservableCollection<ItemModel>>(Tasks
                    .Where(t => t.Title.ToLowerInvariant().Contains(currentText.ToLowerInvariant()))
                    .OrderBy(t => t.Title)
                    .Take(10));

            TaskAutoSuggestBoxItems = filteredItems;
        }

        public void OnTaskAutoSuggestBoxQuerySubmitted(ItemModel selectedItem)
        {
            if (selectedItem == null)
                return;

            MarkAsSelectedAllTasks(false);

            var task = Tasks.FirstOrDefault(t => t.TaskID == selectedItem.ItemID);
            task.IsSelected = true;
        }

        public void OnTaskSaved(TaskItemViewModel task)
        {
            var modifiedTask = Tasks?.FirstOrDefault(t => t.TaskID == task.TaskID);
            if (modifiedTask != null)
            {
                Tasks.Remove(modifiedTask);
            }
            //TODO: I should show a different list for completed tasks
            //if (task.TaskStatus == GoogleTaskStatus.NEEDS_ACTION)
            Tasks.Add(task);
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
            MarkAsSelectedAllTasks(false);
            var task = new TaskItemViewModel
            {
                IsSelected = true
            };
            OnTaskListViewSelectedItem(task);
        }

        public async Task SaveNewTaskListAsync()
        {
            string taskListName = await _dialogService
                .ShowInputStringDialogAsync("Type the task list name", string.Empty, "Save", "Cancel");

            if (string.IsNullOrEmpty(taskListName))
                return;

            ShowTaskListViewProgressRing = true;
            var response = await _googleApiService
                .TaskListService
                .SaveAsync(new GoogleTaskListModel
                {
                    Title = taskListName,
                    UpdatedAt = DateTime.Now
                });
            ShowTaskListViewProgressRing = false;
            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't create the task list. Error code = {response.Errors.ApiError.Code}," +
                    $" message = {response.Errors.ApiError.Message}");
                return;
            }
            await _dialogService.ShowMessageDialogAsync("Succeed", "Task list created.");
            _messenger.Send(
                _mapper.Map<TaskListItemViewModel>(response.Result),
                $"{MessageType.TASK_LIST_ADDED}");
        }

        public async Task ChangeTaskStatusAsync(TaskItemViewModel task, GoogleTaskStatus  taskStatus, bool isSubTask)
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
            var response = await _googleApiService
                .TaskService
                .ChangeStatus(_currentTaskList.TaskListID, task.TaskID, taskStatus);

            if (!response.Succeed)
            {
                ShowTaskListViewProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to mark the task as {statusMessage}.",
                    $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}");
                return;
            }
            var t = _mapper.Map<TaskItemViewModel>(response.Result);
            OnTaskStatusChanged(t, isSubTask);

            _messenger.Send(
                new Tuple<TaskItemViewModel, bool>(t, isSubTask), 
                $"{MessageType.TASK_STATUS_CHANGED_FROM_CONTENT_FRAME}");

            if (!isSubTask && task.HasSubTasks)
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
            foreach (var task in selectedTasks)
            {
                var response = await _googleApiService
                    .TaskService
                    .ChangeStatus(CurrentTaskList.TaskListID, task.TaskID, taskStatus);

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
                var response = await _googleApiService
                    .TaskService
                    .ChangeStatus(_currentTaskList.TaskListID, task.TaskID, taskStatus);

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

        public async Task DeleteTaskAsync(string taskID)
        {
            bool deleteTask = await _dialogService.ShowConfirmationDialogAsync(
                "Confirmation",
                "Are you sure you wanna delete this task?",
                "Yes",
                "No");

            if (!deleteTask)
                return;
            var taskToDelete = Tasks.FirstOrDefault(t => t.TaskID == taskID);
            if (taskToDelete == null)
                throw new KeyNotFoundException($"Couldn't find a task with the id {taskID}");

            _messenger.Send(true, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");
            ShowTaskListViewProgressRing = true;
            var response = await _googleApiService
                .TaskService.DeleteAsync(CurrentTaskList.TaskListID, taskID);
            ShowTaskListViewProgressRing = false;
            _messenger.Send(false, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the selected task. Error code = {response.Errors.ApiError.Code}," +
                    $" message = {response.Errors.ApiError.Message}");
                return;
            }
            Tasks.Remove(taskToDelete);
            _messenger.Send(taskToDelete.TaskID, $"{MessageType.TASK_DELETED_FROM_CONTENT_FRAME}");
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

            var tasksNotRemoved = new List<string>();
            var tasksRemoved = new List<string>();
            foreach (var task in tasksToDelete)
            {
                var response = await _googleApiService
                     .TaskService
                     .DeleteAsync(CurrentTaskList.TaskListID, task.TaskID);

                if (!response.Succeed)
                    tasksNotRemoved.Add(task.Title);
                else
                    tasksRemoved.Add(task.TaskID);
            }
            ShowTaskListViewProgressRing = false;
            _messenger.Send(false, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");

            if (tasksRemoved.Count > 0)
                _messenger.Send(
                    string.Join(",", tasksRemoved),
                    $"{MessageType.TASK_DELETED_FROM_CONTENT_FRAME}");

            Tasks.RemoveAll(t => tasksRemoved.Any(tr => t.TaskID == tr));

            if (tasksNotRemoved.Count > 0)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An error occurred, coudln't delete tasks: {string.Join(",", tasksNotRemoved)}");
                return;
            }
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

            var task = _mapper.Map<GoogleTaskModel>(selectedTask);

            ShowTaskListViewProgressRing = true;
            var response = await _googleApiService
                .TaskService
                .MoveAsync(task, _currentTaskList.TaskListID, selectedTaskList.TaskListID);

            if (!response.Succeed)
            {
                ShowTaskListViewProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to move the selected task from {_currentTaskList.Title} to {selectedTaskList.Title}",
                    $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}");
                return;
            }

            if (selectedTask.HasSubTasks)
            {
                selectedTask.SubTasks.ForEach(st => st.ParentTask = response.Result.TaskID);
                await MoveSubTasksAsync(selectedTaskList.TaskListID, selectedTask.SubTasks);
            }
            ShowTaskListViewProgressRing = false;
            SelectedTaskToMove = null;

            Tasks.Remove(selectedTask);

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
            foreach (var selectedTask in selectedTasks)
            {
                var task = _mapper.Map<GoogleTaskModel>(selectedTask);

                var response = await _googleApiService
                    .TaskService
                    .MoveAsync(task, _currentTaskList.TaskListID, selectedTaskList.TaskListID);

                if (!response.Succeed)
                    tasksNotMoved.Add(selectedTask.Title);
                else
                {
                    tasksMoved.Add(selectedTask.TaskID);
                    if (selectedTask.HasSubTasks)
                    {
                        selectedTask.SubTasks.ForEach(st => st.ParentTask = response.Result.TaskID);
                        await MoveSubTasksAsync(selectedTaskList.TaskListID, selectedTask.SubTasks);
                    }
                }
            }
            ShowTaskListViewProgressRing = false;
            SelectedTaskToMove = null;

            if (tasksMoved.Count > 0)
                _messenger.Send(
                    string.Join(",", tasksMoved),
                    $"{MessageType.TASK_DELETED_FROM_CONTENT_FRAME}");

            Tasks.RemoveAll(t => tasksMoved.Any(tr => t.TaskID == tr));

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
            GoogleResponseModel<GoogleTaskModel> response;
            var stList = new List<string>();
            foreach (var subTask in subTasks)
            {
                var st = _mapper.Map<GoogleTaskModel>(subTask);
                var lastStID = stList.LastOrDefault();
                response = await _googleApiService
                      .TaskService
                      .MoveAsync(st, _currentTaskList.TaskListID, taskListID, st.ParentTask, lastStID);
                if (response.Succeed)
                    stList.Add(response.Result.TaskID);
            }
        }

        public void SortTasks(TaskSortType sortType)
        {
            if (Tasks == null)
                return;
            switch (sortType)
            {
                case TaskSortType.BY_NAME_ASC:
                    Tasks = new ObservableCollection<TaskItemViewModel>(Tasks.OrderBy(t => t.Title));
                    break;
                case TaskSortType.BY_NAME_DESC:
                    Tasks = new ObservableCollection<TaskItemViewModel>(Tasks.OrderByDescending(t => t.Title));
                    break;
                case TaskSortType.BY_UPDATED_DATE_ASC:
                    Tasks = new ObservableCollection<TaskItemViewModel>(Tasks.OrderBy(t => t.UpdatedAt));
                    break;
                case TaskSortType.BY_UPDATED_DATE_DESC:
                    Tasks = new ObservableCollection<TaskItemViewModel>(Tasks.OrderByDescending(t => t.UpdatedAt));
                    break;
                case TaskSortType.CUSTOM_ASC:
                    Tasks = new ObservableCollection<TaskItemViewModel>(Tasks.OrderBy(t => t.Position));
                    break;
                case TaskSortType.CUSTOM_DESC:
                    Tasks = new ObservableCollection<TaskItemViewModel>(Tasks.OrderByDescending(t => t.Position));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("The TaskSortType doesnt have a default sort type");
            }
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

        private void MarkAsSelectedAllTasks(bool isSelected)
            => Tasks?.ForEach(t => t.IsSelected = isSelected);

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