using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
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
    public class NewTaskPageViewModel : ViewModelBase
    {
        #region Members
        private readonly ICustomDialogService _dialogService;
        private readonly IMessenger _messenger;
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IMapper _mapper;

        private string _taskOperationTitle;
        private TaskListItemViewModel _currentTaskList;
        private TaskItemViewModel _currentTask;
        private DateTimeOffset _minDate = DateTime.Now;
        private bool _showTaskProgressRing;
        private bool _isCurrentTaskTitleFocused;
        private ObservableCollection<TaskListItemViewModel> _taskLists = new ObservableCollection<TaskListItemViewModel>();
        private TaskListItemViewModel _selectedTaskList;
        #endregion

        #region Properties
        public string TaskOperationTitle
        {
            get { return _taskOperationTitle; }
            set { Set(ref _taskOperationTitle, value); }
        }

        public TaskItemViewModel CurrentTask
        {
            get { return _currentTask; }
            set { Set(ref _currentTask, value); }
        }

        public DateTimeOffset MinDate
        {
            get { return _minDate; }
            set { Set(ref _minDate, value); }
        }

        public bool ShowTaskProgressRing
        {
            get { return _showTaskProgressRing; }
            set { Set(ref _showTaskProgressRing, value); }
        }

        public bool IsCurrentTaskTitleFocused
        {
            get { return _isCurrentTaskTitleFocused; }
            set { Set(ref _isCurrentTaskTitleFocused, value); }
        }

        public ObservableCollection<TaskListItemViewModel> TaskLists
        {
            get { return _taskLists; }
            set { Set(ref _taskLists, value); }
        }

        public TaskListItemViewModel SelectedTaskList
        {
            get { return _selectedTaskList; }
            set { Set(ref _selectedTaskList, value); }
        }
        #endregion

        #region Commands
        public ICommand SaveChangesCommand { get; set; }

        public ICommand DeleteTaskCommand { get; set; }

        public ICommand MarkAsCompletedCommand { get; set; }

        public ICommand ClosePaneCommand { get; set; }

        public ICommand NewSubTaskCommand { get; set; }

        public ICommand DeleteSubTaskCommand { get; set; }

        public ICommand MarkSubTaskAsCompletedCommand { get; set; }

        public ICommand MarkSubTaskAsIncompletedCommand { get; set; }
        #endregion

        #region Constructor
        public NewTaskPageViewModel(
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
        }
        #endregion

        #region Methods
        private void RegisterMessages()
        {
            _messenger.Register<TaskListItemViewModel>(
                this,
                $"{MessageType.NAVIGATIONVIEW_SELECTION_CHANGED}",
                (taskList) =>
                {
                    _currentTaskList = taskList;
                    _messenger.Send(false, $"{MessageType.OPEN_PANE}");
                });
            _messenger.Register<TaskItemViewModel>(
                this,
                $"{MessageType.NEW_TASK}",
                (task) => InitView(task));
            _messenger.Register<string>(
                this,
                $"{MessageType.TASK_DELETED_FROM_CONTENT_FRAME}",
                OnTaskRemoved);
            _messenger.Register<bool>(
                this,
                $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}",
                (show) => ShowTaskProgressRing = show);
            _messenger.Register<Tuple<TaskItemViewModel, bool>>(
                this,
                $"{MessageType.TASK_STATUS_CHANGED_FROM_CONTENT_FRAME}",
                (tuple) => OnTaskStatusChanged(tuple.Item1, tuple.Item2));
        }

        private void SetCommands()
        {
            SaveChangesCommand = new RelayCommand
                (async () => await SaveChangesAsync());

            DeleteTaskCommand = new RelayCommand
                (async () => await DeleteTaskAsync());

            MarkAsCompletedCommand = new RelayCommand(async () =>
            {
                await ChangeTaskStatusAsync(CurrentTask, GoogleTaskStatus.COMPLETED);
                if (CurrentTask.HasSubTasks)
                {
                    foreach (var st in CurrentTask.SubTasks)
                        await ChangeTaskStatusAsync(st, GoogleTaskStatus.COMPLETED);
                }
            });

            ClosePaneCommand = new RelayCommand(CleanPanel);

            NewSubTaskCommand = new RelayCommand(NewSubTaskAsync);

            DeleteSubTaskCommand = new RelayCommand<TaskItemViewModel>
                (async (subTask) => await DeleteSubTaskAsync(subTask));

            MarkSubTaskAsCompletedCommand = new RelayCommand<TaskItemViewModel>
                (async (subTask) => await ChangeTaskStatusAsync(subTask, GoogleTaskStatus.COMPLETED));

            MarkSubTaskAsIncompletedCommand = new RelayCommand<TaskItemViewModel>
                (async (subTask) => await ChangeTaskStatusAsync(subTask, GoogleTaskStatus.NEEDS_ACTION));
        }

        public async void InitView(TaskItemViewModel task)
        {
            CurrentTask = new TaskItemViewModel
            {
                TaskID = task.TaskID,
                Title = string.IsNullOrEmpty(task.Title) ? "Task title" : task.Title,
                Notes = string.IsNullOrEmpty(task.Notes) ? "Task body" : task.Notes,
                CompletedOn = task.CompletedOn,
                IsDeleted = task.IsDeleted,
                IsHidden = task.IsHidden,
                ParentTask = task.ParentTask,
                Position = task.Position,
                SelfLink = task.SelfLink,
                Status = task.Status,
                ToBeCompletedOn = task.ToBeCompletedOn,
                UpdatedAt = task.UpdatedAt,
                SubTasks = _mapper.Map<ObservableCollection<TaskItemViewModel>>(task.SubTasks),
                Validator = i =>
                {
                    var u = i as TaskItemViewModel;
                    if (string.IsNullOrEmpty(u.Title) || u.Title.Length < 2)
                    {
                        u.Properties[nameof(u.Title)].Errors.Add("Title is required");
                    }
                    if (string.IsNullOrEmpty(u.Notes) || u.Notes.Length < 2)
                    {
                        u.Properties[nameof(u.Notes)].Errors.Add("Notes are required");
                    }
                }
            };
            UpdateTaskOperationTitle(CurrentTask.IsNew);
            //IsCurrentTaskTitleFocused = true;
            //CurrentTask.Validate();

            await GetAllTaskListAsync();
        }

        private async Task SaveChangesAsync()
        {
            var task = _mapper.Map<GoogleTaskModel>(CurrentTask);
            task.UpdatedAt = DateTime.Now;
            bool moveToDifferentTaskList = SelectedTaskList.TaskListID != _currentTaskList.TaskListID;
            bool isNewTask = string.IsNullOrEmpty(task.TaskID);
            if (isNewTask)
                task.Status = GoogleTaskStatus.NEEDS_ACTION.GetString();

            if (SelectedTaskList?.TaskListID == null || _currentTaskList?.TaskListID == null)
            {
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to {(string.IsNullOrEmpty(task.TaskID) ? "save" : "update")} the task.",
                    $"The selected task list and the current task list cant be null");
                return;
            }

            GoogleResponseModel<GoogleTaskModel> response;
            ShowTaskProgressRing = true;
            var subTasksToSave = GetSubTasksToSave(isNewTask, moveToDifferentTaskList);
            var currentSts = GetCurrentSubTasks();

            //If the task selected in the combo is not the same as the one in the 
            //navigation view, its because we are trying to save/update a 
            //task into a different task list
            if (moveToDifferentTaskList)
            {
                if (isNewTask)
                {
                    response = await _googleApiService
                        .TaskService
                        .SaveAsync(SelectedTaskList.TaskListID, task);
                    if (!response.Succeed)
                    {
                        await _dialogService.ShowMessageDialogAsync(
                            $"An error occurred while trying to seve the task into {SelectedTaskList.Title}.",
                            $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}");
                        return;
                    }
                    _messenger.Send(false, $"{MessageType.OPEN_PANE}");

                    subTasksToSave.ForEach(st => st.ParentTask = response.Result.TaskID);

                    await SaveSubTasksAsync(
                        subTasksToSave,
                        isNewTask,
                        moveToDifferentTaskList,
                        Enumerable.Empty<TaskItemViewModel>().ToList());

                    await _dialogService.ShowMessageDialogAsync(
                        "Succeed",
                        $"The task was sucessfully created into {SelectedTaskList.Title}");
                    return;
                }
                else
                {
                    await MoveCurrentTaskAsync(task);
                    return;
                }
            }
            else if (isNewTask)
            {
                response = await _googleApiService
                    .TaskService
                    .SaveAsync(_currentTaskList.TaskListID, task);
            }
            else
            {
                response = await _googleApiService
                    .TaskService
                    .UpdateAsync(_currentTaskList.TaskListID, task.TaskID, task);
            }
            ShowTaskProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to {(string.IsNullOrEmpty(task.TaskID) ? "save" : "update")} the task.",
                    $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}");
                return;
            }

            CurrentTask = _mapper.Map<TaskItemViewModel>(response.Result);

            var sts = await SaveSubTasksAsync(subTasksToSave, isNewTask, moveToDifferentTaskList, currentSts);
            CurrentTask.SubTasks = new ObservableCollection<TaskItemViewModel>(sts);

            _messenger.Send(CurrentTask, $"{MessageType.TASK_SAVED}");
            UpdateTaskOperationTitle(isNewTask);
        }

        public async Task DeleteTaskAsync()
        {
            bool deleteTask = await _dialogService.ShowConfirmationDialogAsync(
                "Confirmation",
                "Are you sure you wanna delete this task?",
                "Yes",
                "No");

            if (!deleteTask)
                return;

            ShowTaskProgressRing = true;
            var response = await _googleApiService
                .TaskService.DeleteAsync(_currentTaskList.TaskListID, CurrentTask.TaskID);
            ShowTaskProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the selected task. Error code = {response.Errors.ApiError.Code}," +
                    $" message = {response.Errors.ApiError.Message}");
                return;
            }

            _messenger.Send(CurrentTask.TaskID, $"{MessageType.TASK_DELETED}");
            CleanPanel();
        }

        public async Task ChangeTaskStatusAsync(TaskItemViewModel task, GoogleTaskStatus taskStatus)
        {
            string statusMessage =
                $"{(taskStatus == GoogleTaskStatus.COMPLETED ? "completed" : "incompleted")}";

            bool changeStatus = await _dialogService.ShowConfirmationDialogAsync(
                "Confirmation",
                $"Mark {task.Title} as {statusMessage}?",
                "Yes",
                "No");
            if (!changeStatus)
                return;

            ShowTaskProgressRing = true;
            var response = await _googleApiService
                .TaskService
                .ChangeStatus(_currentTaskList.TaskListID, task.TaskID, taskStatus);
            ShowTaskProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to mark {task.Title} as {statusMessage}.",
                    $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}");
                return;
            }

            task.Status = response.Result.Status;
            task.CompletedOn = response.Result.CompletedOn;
            task.UpdatedAt = response.Result.UpdatedAt;

            _messenger.Send(
                new Tuple<TaskItemViewModel, bool>(task, task.HasParentTask),
                $"{MessageType.TASK_STATUS_CHANGED_FROM_PANE_FRAME}");

            await _dialogService.ShowMessageDialogAsync(
                "Succeed",
                $"{task.Title} was marked as {statusMessage}.");
        }

        private void CleanPanel()
        {
            CurrentTask = new TaskItemViewModel
            {
                Title = string.Empty,
                Notes = string.Empty
            };
            _messenger.Send(false, $"{MessageType.OPEN_PANE}");
        }

        private void UpdateTaskOperationTitle(bool isNewTask)
        {
            if (CurrentTask.IsNew)
                TaskOperationTitle = "New Task";
            else
                TaskOperationTitle = "Update task";
        }

        /// <summary>
        /// Cleans the panel if the ids in <paramref name="taskIDs"/> is
        /// in the current task
        /// </summary>
        /// <param name="taskIDs">Comma separated tasks ids</param>
        private void OnTaskRemoved(string taskIDs)
        {
            var removedTasksIds = taskIDs.Split(',');
            if (removedTasksIds.Contains(CurrentTask?.TaskID))
            {
                CleanPanel();
            }
        }

        private void OnTaskStatusChanged(TaskItemViewModel task, bool isSubTask)
        {
            TaskItemViewModel taskFound = null;

            if (!isSubTask)
                taskFound = CurrentTask?.TaskID == task.TaskID ? CurrentTask : null;
            else
            {
                taskFound = CurrentTask?
                    .SubTasks?
                    .FirstOrDefault(st => st.TaskID == task.TaskID);
            }

            if (taskFound == null)
                return;

            taskFound.CompletedOn = task.CompletedOn;
            taskFound.UpdatedAt = task.UpdatedAt;
            taskFound.Status = task.Status;
        }

        private async Task GetAllTaskListAsync()
        {
            ShowTaskProgressRing = true;
            var response = await _googleApiService
                .TaskListService
                .GetAllAsync();
            ShowTaskProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Coudn't get the task lists",
                    $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}");
                return;
            }

            TaskLists = _mapper.Map<ObservableCollection<TaskListItemViewModel>>
                (response.Result.Items.OrderBy(t => t.Title));

            SelectedTaskList = TaskLists
                .FirstOrDefault(t => t.TaskListID == _currentTaskList.TaskListID);
        }

        private async Task MoveCurrentTaskAsync(GoogleTaskModel task)
        {
            ShowTaskProgressRing = true;

            var response = await _googleApiService
                .TaskService
                .MoveAsync(task, _currentTaskList.TaskListID, SelectedTaskList.TaskListID);

            if (!response.Succeed)
            {
                ShowTaskProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to move the selected task from {_currentTaskList.Title} to {SelectedTaskList.Title}",
                    $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}");
                return;
            }
            _messenger.Send(CurrentTask.TaskID, $"{MessageType.TASK_DELETED}");
            _messenger.Send(false, $"{MessageType.OPEN_PANE}");

            var subTasks = GetSubTasksToSave(false, true);

            subTasks.ForEach(st => st.ParentTask = response.Result.TaskID);

            ShowTaskProgressRing = false;

            await SaveSubTasksAsync(subTasks, false, true, Enumerable.Empty<TaskItemViewModel>().ToList());
            await _dialogService.ShowMessageDialogAsync(
                "Succeed",
                $"Task sucessfully moved from: {_currentTaskList.Title} to: {SelectedTaskList.Title}");
        }

        private async void NewSubTaskAsync()
        {
            string subTaskTitle = await _dialogService.ShowInputStringDialogAsync(
                "Type the sub task title",
                string.Empty,
                "Save",
                "Cancel");

            if (string.IsNullOrEmpty(subTaskTitle))
                return;

            if (CurrentTask.SubTasks == null)
                CurrentTask.SubTasks = new ObservableCollection<TaskItemViewModel>();

            CurrentTask.SubTasks.Add(new TaskItemViewModel
            {
                Title = subTaskTitle,
                UpdatedAt = DateTime.Now,
                Status = GoogleTaskStatus.NEEDS_ACTION.GetString()
            });
            if (!CurrentTask.HasSubTasks)
                CurrentTask.HasSubTasks = true;
        }

        private async Task<IEnumerable<TaskItemViewModel>> SaveSubTasksAsync(
            IEnumerable<TaskItemViewModel> subTasks,
            bool isNewTask,
            bool moveToDifferentTaskList,
            List<TaskItemViewModel> currentSubTasks)
        {
            ShowTaskProgressRing = true;
            GoogleResponseModel<GoogleTaskModel> response;

            foreach (var subTask in subTasks)
            {
                var st = _mapper.Map<GoogleTaskModel>(subTask);
                var lastStID = currentSubTasks.LastOrDefault()?.TaskID;
                if (moveToDifferentTaskList)
                {
                    if (isNewTask)
                        response = await _googleApiService
                            .TaskService
                            .SaveAsync(SelectedTaskList.TaskListID, st, st.ParentTask, lastStID);
                    else
                        response = await _googleApiService
                            .TaskService
                            .MoveAsync(st, _currentTaskList.TaskListID, SelectedTaskList.TaskListID, st.ParentTask, lastStID);
                }
                else
                {
                    response = await _googleApiService
                        .TaskService
                        .SaveAsync(_currentTaskList.TaskListID, st, CurrentTask.TaskID, lastStID);
                }

                if (response.Succeed)
                    currentSubTasks.Add(_mapper.Map<TaskItemViewModel>(response.Result));
            }
            ShowTaskProgressRing = false;

            return currentSubTasks;
        }

        public async Task DeleteSubTaskAsync(TaskItemViewModel subTask)
        {
            bool deleteTask = await _dialogService.ShowConfirmationDialogAsync(
                "Confirmation",
                $"Are you sure you wanna delete {subTask.Title}?",
                "Yes",
                "No");

            if (!deleteTask)
                return;

            if (subTask.IsNew)
            {
                CurrentTask.SubTasks?.Remove(subTask);
                return;
            }

            ShowTaskProgressRing = true;
            var response = await _googleApiService
                .TaskService
                .DeleteAsync(_currentTaskList.TaskListID, subTask.TaskID);
            ShowTaskProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the selected sub task. Error code = {response.Errors.ApiError.Code}," +
                    $" message = {response.Errors.ApiError.Message}");
                return;
            }
            CurrentTask.SubTasks?.Remove(subTask);
            _messenger.Send(
                new KeyValuePair<string, string>(CurrentTask.TaskID, subTask.TaskID),
                $"{MessageType.SUBTASK_DELETED}");
        }

        private List<TaskItemViewModel> GetSubTasksToSave(bool isCurrentTaskNew, bool moveToDifferentTaskList)
        {
            //if the current task is new or the current task is new and we are not moving it to
            //a different task list, so we choose the st that are new and not completed
            if (isCurrentTaskNew || !isCurrentTaskNew && !moveToDifferentTaskList)
                return CurrentTask.SubTasks?
                    .Where(st => st.IsNew && st.CompletedOn == null)
                    .ToList() ??
                    Enumerable.Empty<TaskItemViewModel>()
                        .ToList();
            //else, the current task is not new and we are moving it to
            //a different task list, so we choose all the st
            else
                return CurrentTask.SubTasks?.ToList() ??
                    Enumerable.Empty<TaskItemViewModel>()
                        .ToList();
        }

        private List<TaskItemViewModel> GetCurrentSubTasks()
        {
            return CurrentTask.SubTasks?
                .Where(st => !st.IsNew)
                .ToList() ??
                Enumerable.Empty<TaskItemViewModel>()
                    .ToList();
        }
        #endregion
    }
}
