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
        private readonly IMapper _mapper;
        private readonly IMiraiNotesDataService _dataService;

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

        public ICommand SubTaskSelectedItemCommand { get; set; }
        #endregion

        #region Constructors
        public TasksPageViewModel(
            ICustomDialogService dialogService,
            IMessenger messenger,
            INavigationService navigationService,
            IUserCredentialService userCredentialService,
            IMapper mapper,
            IMiraiNotesDataService dataService)
        {
            _dialogService = dialogService;
            _messenger = messenger;
            _navigationService = navigationService;
            _userCredentialService = userCredentialService;
            _mapper = mapper;
            _dataService = dataService;

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
                ((task) => OnTaskSelectedItem(task));

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
                var taskLists = await _dataService
                    .TaskListService
                    .GetAllAsync();

                TaskLists = _mapper.Map<ObservableCollection<TaskListItemViewModel>>
                    (taskLists
                        .Where(t => t.GoogleTaskListID != _currentTaskList.TaskListID)
                        .OrderBy(t => t.Title));
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
            if (taskList == null)
            {
                OnNoTaskListAvailable();
                return;
            }

            ShowTaskListViewProgressRing = true;

            var response = await _dataService
                .TaskService
                .GetAsNoTrackingAsync(
                    t => t.TaskList.GoogleTaskListID == taskList.TaskListID && 
                        t.LocalStatus != LocalStatus.DELETED,
                    t => t.OrderBy(ta => ta.Position));
            var tasks = _mapper.Map<List<TaskItemViewModel>>(response);            

            if (tasks.Count > 0)
            {
                var mainTasks = tasks
                    .Where(t => t.ParentTask == null);
                mainTasks.ForEach(t =>
                {
                    t.SubTasks = _mapper.Map<ObservableCollection<TaskItemViewModel>>(
                        tasks
                        .Where(st => st.ParentTask == t.TaskID)
                        .OrderBy(st => st.Position));
                });

                Tasks = new ObservableCollection<TaskItemViewModel>(mainTasks);
                TaskAutoSuggestBoxItems = _mapper.Map<ObservableCollection<ItemModel>>(mainTasks.OrderBy(t => t.Title));
            }
            else
            {
                Tasks = new ObservableCollection<TaskItemViewModel>();
                TaskAutoSuggestBoxItems = new ObservableCollection<ItemModel>();
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

        public void OnTaskSelectedItem(TaskItemViewModel task)
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

            var selectedSubTask = GetSelectedSubTask();
            selectedSubTask.ForEach(st => st.IsSelected = false);

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
            MarkAsSelectedAllTasks(false);
            var task = new TaskItemViewModel
            {
                IsSelected = true
            };
            OnTaskSelectedItem(task);
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
                    $"Coudln't create the task list. Error code = {response.Message}");
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
            var taskToUpdate = await _dataService
                .TaskService
                .FirstOrDefaultAsNoTrackingAsync(tx => tx.GoogleTaskID == task.TaskID);

            if (taskToUpdate == null)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Task {task.Title} doesnt exists in the db");
                return;
            }

            taskToUpdate.Status = taskStatus.GetString();
            taskToUpdate.UpdatedAt = DateTime.Now;
            taskToUpdate.LocalStatus = LocalStatus.UPDATED;
            taskToUpdate.ToBeSynced = true;

            var response = await _dataService
                .TaskService
                .UpdateAsync(taskToUpdate);

            if (!response.Succeed)
            {
                ShowTaskListViewProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to mark the task as {statusMessage}.",
                    $"Error: {response.Message}.");
                return;
            }
            var t = _mapper.Map<TaskItemViewModel>(taskToUpdate);
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
            //var tasksToUpdate = await _dataService
            //    .TaskService
            //    .GetAsync(t => selectedTasks.Any(st => st.TaskID == t.GoogleTaskID));

            //tasksToUpdate.ForEach(t =>
            //{
            //    t.Status = taskStatus.GetString();
            //    t.UpdatedAt = DateTime.Now;
            //    t.LocalStatus = LocalStatus.UPDATED;
            //    t.ToBeSynced = true;
            //});

            //var response = await _dataService
            //    .TaskService
            //    .UpdateRangeAsync(tasksToUpdate)

            foreach (var task in selectedTasks)
            {
                var taskToUpdate = await _dataService
                    .TaskService
                    .FirstOrDefaultAsNoTrackingAsync(ta => ta.GoogleTaskID == task.TaskID);

                taskToUpdate.Status = taskStatus.GetString();
                taskToUpdate.UpdatedAt = DateTime.Now;
                taskToUpdate.LocalStatus = LocalStatus.UPDATED;
                taskToUpdate.ToBeSynced = true;

                var response = await _dataService
                    .TaskService
                    .UpdateAsync(taskToUpdate);

                if (!response.Succeed)
                {
                    tasksStatusNotChanged.Add(task.Title);
                    continue;
                }
                var t = _mapper.Map<TaskItemViewModel>(taskToUpdate);
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
                var taskToUpdate = await _dataService
                    .TaskService
                    .FirstOrDefaultAsNoTrackingAsync(ta => ta.GoogleTaskID == task.TaskID);

                taskToUpdate.Status = taskStatus.GetString();
                taskToUpdate.UpdatedAt = DateTime.Now;
                taskToUpdate.LocalStatus = LocalStatus.UPDATED;
                taskToUpdate.ToBeSynced = true;

                var response = await _dataService
                    .TaskService
                    .UpdateAsync(taskToUpdate);

                if (!response.Succeed)
                {
                    tasksStatusNotChanged.Add(task.Title);
                    continue;
                }

                var t = _mapper.Map<TaskItemViewModel>(taskToUpdate);
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

            var entity = await _dataService
                .TaskService
                .FirstOrDefaultAsNoTrackingAsync(t => t.GoogleTaskID == taskID);

            if (entity == null)
            {
                ShowTaskListViewProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    "Task couldn't be found in the db");
                return;
            }

            entity.LocalStatus = LocalStatus.DELETED;
            entity.UpdatedAt = DateTime.Now;
            entity.ToBeSynced = true;

            var response = await _dataService
                .TaskService
                .UpdateAsync(entity);

            ShowTaskListViewProgressRing = false;
            _messenger.Send(false, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the selected task. Error code = {response.Message}");
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

            var tasks = await _dataService
                .TaskService
                .GetAsNoTrackingAsync(
                    t => tasksToDelete.Any(td => t.GoogleTaskID == td.TaskID),
                    null,
                    string.Empty);

            tasks.ForEach(t =>
            {
                t.UpdatedAt = DateTime.Now;
                t.LocalStatus = LocalStatus.DELETED;
                t.ToBeSynced = true;
            });

            var response = await _dataService
                .TaskService
                .UpdateRangeAsync(tasks);

            ShowTaskListViewProgressRing = false;
            _messenger.Send(false, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}");

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An error occurred, coudln't delete the selected tasks");
                return;
            }

            _messenger.Send(
                string.Join(",", tasksToDelete.Select(t => t.TaskID)),
                $"{MessageType.TASK_DELETED_FROM_CONTENT_FRAME}");

            Tasks.RemoveAll(t => tasksToDelete.Any(tr => t.TaskID == tr.TaskID));
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

            var oldEntity = await _dataService
                .TaskService
                .FirstOrDefaultAsNoTrackingAsync(t => t.GoogleTaskID == task.TaskID);

            if (oldEntity == null)
            {
                ShowTaskListViewProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    "Couldn't find the task to delete in the move process in the db");
                return;
            }

            oldEntity.LocalStatus = LocalStatus.DELETED;
            oldEntity.ToBeSynced = true;
            oldEntity.UpdatedAt = DateTime.Now;

            var response = await _dataService
                .TaskService
                .UpdateAsync(oldEntity);

            if (!response.Succeed)
            {
                ShowTaskListViewProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    "Couldn't mark as deleted the task in the db");
                return;
            }

            var entity = new GoogleTask
            {
                CompletedOn = task.CompletedOn,
                CreatedAt = DateTime.Now,
                GoogleTaskID = Guid.NewGuid().ToString(),
                IsDeleted = task.IsDeleted,
                IsHidden = task.IsHidden,
                LocalStatus = LocalStatus.CREATED,
                Notes = task.Notes,
                ParentTask = task.ParentTask,
                Position = task.Position,
                Status = task.Status,
                Title = task.Title,
                ToBeCompletedOn = task.ToBeCompletedOn,
                ToBeSynced = true,
                UpdatedAt = DateTime.Now
            };

            response = await _dataService
                .TaskService
                .AddAsync(selectedTaskList.TaskListID, entity);

            if (!response.Succeed)
            {
                ShowTaskListViewProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to move the selected task from {_currentTaskList.Title} to {selectedTaskList.Title}",
                    $"Error: {response.Message}.");
                return;
            }

            if (selectedTask.HasSubTasks)
            {
                selectedTask.SubTasks.ForEach(st => st.ParentTask = entity.GoogleTaskID);
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

            var oldEntities = await _dataService
                .TaskService
                .GetAsNoTrackingAsync(
                    t => selectedTasks.Any(st => st.TaskID == t.GoogleTaskID),
                    null,
                    string.Empty);

            oldEntities.ForEach(t =>
            {
                t.LocalStatus = LocalStatus.DELETED;
                t.ToBeSynced = true;
                t.UpdatedAt = DateTime.Now;
            });

            var response = await _dataService
                .TaskService
                .UpdateRangeAsync(oldEntities);
            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    "Couldnt delete the selected task in the move operation");
                return;
            }
            var tasksNotMoved = new List<string>();
            var tasksMoved = new List<string>();

            selectedTasks.ForEach(async st =>
            {
                var entity = new GoogleTask
                {
                    CompletedOn = st.CompletedOn,
                    CreatedAt = DateTime.Now,
                    GoogleTaskID = Guid.NewGuid().ToString(),
                    IsDeleted = st.IsDeleted,
                    IsHidden = st.IsHidden,
                    LocalStatus = LocalStatus.CREATED,
                    Notes = st.Notes,
                    ParentTask = st.ParentTask,
                    Position = st.Position,
                    Status = st.Status,
                    Title = st.Title,
                    ToBeCompletedOn = st.ToBeCompletedOn,
                    ToBeSynced = true,
                    UpdatedAt = DateTime.Now
                };

                response = await _dataService
                    .TaskService
                    .AddAsync(selectedTaskList.TaskListID, entity);

                if (!response.Succeed)
                    tasksNotMoved.Add(st.Title);
                else
                {
                    tasksMoved.Add(st.TaskID);
                    if (st.HasSubTasks)
                    {
                        st.SubTasks.ForEach(s => s.ParentTask = entity.GoogleTaskID);
                        await MoveSubTasksAsync(selectedTaskList.TaskListID, st.SubTasks);
                    }
                }
            });

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
            var oldEntities = await _dataService
                .TaskService
                .GetAsNoTrackingAsync(
                    st => subTasks.Any(subt => subt.TaskID == st.GoogleTaskID),
                    null,
                    string.Empty);

            oldEntities.ForEach(t =>
            {
                t.LocalStatus = LocalStatus.DELETED;
                t.ToBeSynced = true;
                t.UpdatedAt = DateTime.Now;
            });
            var response = await _dataService
                .TaskService
                .UpdateRangeAsync(oldEntities);
            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    "Couldnt delete the selected sub task in the move operation");
                return;
            }

            var stList = new List<string>();
            foreach (var subTask in subTasks)
            {
                var entity = new GoogleTask
                {
                    CompletedOn = subTask.CompletedOn,
                    CreatedAt = DateTime.Now,
                    GoogleTaskID = Guid.NewGuid().ToString(),
                    IsDeleted = subTask.IsDeleted,
                    IsHidden = subTask.IsHidden,
                    LocalStatus = LocalStatus.CREATED,
                    Notes = subTask.Notes,
                    ParentTask = subTask.ParentTask,
                    Position = stList.LastOrDefault(),
                    Status = subTask.Status,
                    Title = subTask.Title,
                    ToBeCompletedOn = subTask.ToBeCompletedOn,
                    ToBeSynced = true,
                    UpdatedAt = DateTime.Now
                };

                response = await _dataService
                    .TaskService
                    .AddAsync(taskListID, entity);

                if (response.Succeed)
                    stList.Add(entity.GoogleTaskID);
            }
        }

        public void SortTasks(TaskSortType sortType)
        {
            if (Tasks == null)
                return;
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
            var task = Tasks?.SelectMany(t => t.SubTasks?.Where(st => st.IsSelected));
            return task;
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