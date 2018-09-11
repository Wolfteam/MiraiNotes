using AutoMapper;
using GalaSoft.MvvmLight;
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
    public class NewTaskPageViewModel : ViewModelBase
    {
        #region Members
        private readonly ICustomDialogService _dialogService;
        private readonly IMessenger _messenger;
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IMapper _mapper;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDispatcherHelper _dispatcherHelper;


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
            IMapper mapper,
            IMiraiNotesDataService dataService,
            IDispatcherHelper dispatcherHelper)
        {
            _dialogService = dialogService;
            _messenger = messenger;
            _navigationService = navigationService;
            _userCredentialService = userCredentialService;
            _mapper = mapper;
            _dataService = dataService;
            _dispatcherHelper = dispatcherHelper;

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
                async (task) =>
                {
                    await GetAllTaskListAsync();
                    await InitView(task.TaskID);
                    CurrentTask.Validator = i =>
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
                    };

                    UpdateTaskOperationTitle(CurrentTask.IsNew, CurrentTask.HasParentTask);
                    //IsCurrentTaskTitleFocused = true;
                    CurrentTask.Validate();
                });
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

            NewSubTaskCommand = new RelayCommand
                (async () => await NewSubTaskAsync());

            DeleteSubTaskCommand = new RelayCommand<TaskItemViewModel>
                (async (subTask) => await DeleteSubTaskAsync(subTask));

            MarkSubTaskAsCompletedCommand = new RelayCommand<TaskItemViewModel>
                (async (subTask) => await ChangeTaskStatusAsync(subTask, GoogleTaskStatus.COMPLETED));

            MarkSubTaskAsIncompletedCommand = new RelayCommand<TaskItemViewModel>
                (async (subTask) => await ChangeTaskStatusAsync(subTask, GoogleTaskStatus.NEEDS_ACTION));
        }

        public async Task InitView(string taskID)
        {
            ShowTaskProgressRing = true;
            if (string.IsNullOrEmpty(taskID))
                CurrentTask = new TaskItemViewModel();
            else
            {
                var ta = await _dataService
                    .TaskService
                    .FirstOrDefaultAsNoTrackingAsync(x => x.GoogleTaskID == taskID);

                var sts = await _dataService
                    .TaskService
                    .GetAsNoTrackingAsync(
                        st => st.ParentTask == taskID,
                        st => st.OrderBy(x => x.Position));

                if (!ta.Succeed || !sts.Succeed)
                {
                    ShowTaskProgressRing = false;
                    await _dialogService.ShowMessageDialogAsync(
                        "Error",
                        $"An unexpected error occurred. Error = {ta.Message} {sts.Message}");
                    return;
                }

                var t = _mapper.Map<TaskItemViewModel>(ta.Result);
                t.SubTasks = _mapper.Map<ObservableCollection<TaskItemViewModel>>(sts.Result);
                CurrentTask = t;
            }
            ShowTaskProgressRing = false;
        }

        private async Task SaveChangesAsync()
        {
            bool isNewTask = CurrentTask.IsNew;

            if (SelectedTaskList?.TaskListID == null || _currentTaskList?.TaskListID == null)
            {
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to {(isNewTask ? "save" : "update")} the task.",
                    $"The selected task list and the current task list cant be null");
                return;
            }
            //If the task selected in the combo is not the same as the one in the 
            //navigation view, its because we are trying to save/update a 
            //task into a different task list
            bool moveToDifferentTaskList = SelectedTaskList.TaskListID != _currentTaskList.TaskListID;
            if (moveToDifferentTaskList && !isNewTask)
            {
                await MoveCurrentTaskAsync();
                return;
            }

            GoogleTask entity;
            ShowTaskProgressRing = true;
            if (isNewTask)
                entity = new GoogleTask();
            else
            {
                var dbResponse = await _dataService
                    .TaskService
                    .FirstOrDefaultAsNoTrackingAsync(t => t.GoogleTaskID == CurrentTask.TaskID);

                if (!dbResponse.Succeed || dbResponse.Result == null)
                {
                    ShowTaskProgressRing = false;
                    await _dialogService.ShowMessageDialogAsync(
                        "Error",
                        $"Couldn't find the task to update from db. Error = {dbResponse.Message}");
                    return;
                }
                entity = dbResponse.Result;
            }
            if (!moveToDifferentTaskList || moveToDifferentTaskList && isNewTask)
            {
                if (isNewTask)
                    entity.CreatedAt = DateTime.Now;
                entity.CompletedOn = CurrentTask.CompletedOn;
                entity.GoogleTaskID = CurrentTask.IsNew ?
                    Guid.NewGuid().ToString() : CurrentTask.TaskID;
                entity.IsDeleted = CurrentTask.IsDeleted;
                entity.IsHidden = CurrentTask.IsHidden;
                entity.Notes = CurrentTask.Notes;
                entity.ParentTask = CurrentTask.ParentTask;
                entity.Position = CurrentTask.Position;
                entity.Status = CurrentTask.IsNew ?
                    GoogleTaskStatus.NEEDS_ACTION.GetString() : CurrentTask.Status;
                entity.Title = CurrentTask.Title;
                entity.LocalStatus = CurrentTask.IsNew ?
                    LocalStatus.CREATED :
                    entity.LocalStatus == LocalStatus.CREATED ?
                        LocalStatus.CREATED :
                        LocalStatus.UPDATED;
                entity.ToBeSynced = true;
                entity.UpdatedAt = DateTime.Now;
                entity.ToBeCompletedOn = CurrentTask.ToBeCompletedOn;
            }

            Response<GoogleTask> response;
            var subTasksToSave = GetSubTasksToSave(isNewTask, moveToDifferentTaskList);
            var currentSts = GetCurrentSubTasks();
            //If we are creating a new task but in a different tasklist
            if (moveToDifferentTaskList)
            {
                response = await _dataService
                     .TaskService
                     .AddAsync(SelectedTaskList.TaskListID, entity);

                if (!response.Succeed)
                {
                    ShowTaskProgressRing = false;
                    await _dialogService.ShowMessageDialogAsync(
                        $"An error occurred while trying to seve the task into {SelectedTaskList.Title}.",
                        $"Error = {response.Message}.");
                    return;
                }
                _messenger.Send(false, $"{MessageType.OPEN_PANE}");

                subTasksToSave.ForEach(st => st.ParentTask = entity.GoogleTaskID);

                await SaveSubTasksAsync(
                    subTasksToSave,
                    isNewTask,
                    moveToDifferentTaskList,
                    Enumerable.Empty<TaskItemViewModel>().ToList());

                _messenger.Send(
                    $"The task was sucessfully created into {SelectedTaskList.Title}",
                    $"{MessageType.SHOW_IN_APP_NOTIFICATION}");
                return;
            }
            else if (isNewTask)
                response = await _dataService
                    .TaskService
                    .AddAsync(_currentTaskList.TaskListID, entity);
            else
                response = await _dataService
                    .TaskService
                    .UpdateAsync(entity);

            ShowTaskProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to {(isNewTask ? "save" : "update")} the task.",
                    $"Error: {response.Message}.");
                return;
            }

            CurrentTask = _mapper.Map<TaskItemViewModel>(response.Result);

            var sts = await SaveSubTasksAsync(subTasksToSave, isNewTask, moveToDifferentTaskList, currentSts);

            CurrentTask.SubTasks = new ObservableCollection<TaskItemViewModel>(sts);

            _messenger.Send(CurrentTask.TaskID, $"{MessageType.TASK_SAVED}");
            UpdateTaskOperationTitle(isNewTask, CurrentTask.HasParentTask);
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

            var deleteResponse = await _dataService
                .TaskService
                .RemoveTaskAsync(CurrentTask.TaskID);

            ShowTaskProgressRing = false;

            if (!deleteResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the selected task. Error = {deleteResponse.Message}.");
                return;
            }
            //If we are deleting a subtask
            if (CurrentTask.HasParentTask)
                _messenger.Send(
                     new KeyValuePair<string, string>(CurrentTask.ParentTask, CurrentTask.TaskID),
                     $"{MessageType.SUBTASK_DELETED_FROM_PANE_FRAME}");
            else
                _messenger.Send(
                    CurrentTask.TaskID,
                    $"{MessageType.TASK_DELETED_FROM_PANE_FRAME}");
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

            var response = await _dataService
                .TaskService
                .ChangeTaskStatusAsync(task.TaskID, taskStatus);
            
            ShowTaskProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    $"Error",
                    $"An error occurred while trying to mark {task.Title} as {statusMessage}. " +
                    $"Error = {response.Message}.");
                return;
            }

            task.Status = response.Result.Status;
            task.CompletedOn = response.Result.CompletedOn;
            task.UpdatedAt = response.Result.UpdatedAt;

            _messenger.Send(
                new Tuple<TaskItemViewModel, bool>(task, task.HasParentTask),
                $"{MessageType.TASK_STATUS_CHANGED_FROM_PANE_FRAME}");

            _messenger.Send(
                $"{task.Title} was marked as {statusMessage}.",
                $"{MessageType.SHOW_IN_APP_NOTIFICATION}");
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

        private void UpdateTaskOperationTitle(bool isNewTask, bool isSubTask)
        {
            if (CurrentTask.IsNew && !isSubTask)
                TaskOperationTitle = "New Task";
            else if (!CurrentTask.IsNew && !isSubTask)
                TaskOperationTitle = "Update Task";
            else if (CurrentTask.IsNew && isSubTask)
                TaskOperationTitle = "New Sub Task";
            else
                TaskOperationTitle = "Update Sub Task";
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

            var dbResponse = await _dataService
                .TaskListService
                .GetAsNoTrackingAsync(
                    tl => tl.LocalStatus != LocalStatus.DELETED &&
                        tl.User.IsActive,
                    tl => tl.OrderBy(t => t.Title));

            if (!dbResponse.Succeed)
            {
                ShowTaskProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"An error occurred while trying to retrieve all the task lists. Error = {dbResponse.Message}");
                return;
            }

            TaskLists = _mapper.Map<ObservableCollection<TaskListItemViewModel>>(dbResponse.Result);

            SelectedTaskList = TaskLists
                .FirstOrDefault(t => t.TaskListID == _currentTaskList.TaskListID);
            ShowTaskProgressRing = false;
        }

        private async Task MoveCurrentTaskAsync()
        {
            ShowTaskProgressRing = true;

            var moveResponse = await _dataService
                .TaskService
                .MoveAsync(SelectedTaskList.TaskListID, CurrentTask.TaskID, null, null);

            if (!moveResponse.Succeed)
            {
                ShowTaskProgressRing = false;
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to move the selected task from {_currentTaskList.Title} to {SelectedTaskList.Title}",
                    $"Error: {moveResponse.Message}.");
                return;
            }
            if (!CurrentTask.HasParentTask)
                _messenger.Send(CurrentTask.TaskID, $"{MessageType.TASK_DELETED_FROM_PANE_FRAME}");
            else
                _messenger.Send(
                    new KeyValuePair<string, string>(CurrentTask.ParentTask, CurrentTask.TaskID),
                    $"{MessageType.SUBTASK_DELETED_FROM_PANE_FRAME}");

            _messenger.Send(false, $"{MessageType.OPEN_PANE}");

            var subTasks = GetSubTasksToSave(false, true);

            subTasks.ForEach(st => st.ParentTask = moveResponse.Result.GoogleTaskID);

            ShowTaskProgressRing = false;

            await SaveSubTasksAsync(subTasks, false, true, Enumerable.Empty<TaskItemViewModel>().ToList());

            _messenger.Send(
                $"Task sucessfully moved from: {_currentTaskList.Title} to: {SelectedTaskList.Title}",
                $"{MessageType.SHOW_IN_APP_NOTIFICATION}");
        }

        private async Task NewSubTaskAsync()
        {
            string subTaskTitle = await _dialogService.ShowInputStringDialogAsync(
                "Type the sub task title",
                string.Empty,
                "Save",
                "Cancel");

            if (string.IsNullOrEmpty(subTaskTitle))
                return;

            if (CurrentTask.SubTasks == null)
                CurrentTask.SubTasks = new SmartObservableCollection<TaskItemViewModel>();

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
            IEnumerable<TaskItemViewModel> subTasksToSave,
            bool isNewTask,
            bool moveToDifferentTaskList,
            List<TaskItemViewModel> currentSubTasks)
        {
            ShowTaskProgressRing = true;
            string taskListID = moveToDifferentTaskList ?
                SelectedTaskList.TaskListID :
                _currentTaskList.TaskListID;

            if (moveToDifferentTaskList && !isNewTask)
            {
                foreach (var subTask in subTasksToSave)
                {
                    var lastStID = currentSubTasks.LastOrDefault()?.TaskID;
                    var moveResponse = await _dataService
                        .TaskService
                        .MoveAsync(taskListID, subTask.TaskID, subTask.ParentTask, lastStID);
                    if (moveResponse.Succeed)
                        currentSubTasks.Add(_mapper.Map<TaskItemViewModel>(moveResponse.Result));
                }
            }
            else
            {
                foreach (var subTask in subTasksToSave)
                {
                    var lastStID = currentSubTasks.LastOrDefault()?.TaskID;
                    var entity = new GoogleTask
                    {
                        CompletedOn = subTask.CompletedOn,
                        CreatedAt = DateTime.Now,
                        GoogleTaskID = subTask.IsNew ? Guid.NewGuid().ToString() : subTask.TaskID,
                        IsDeleted = subTask.IsDeleted,
                        IsHidden = subTask.IsHidden,
                        LocalStatus = LocalStatus.CREATED,
                        Notes = subTask.Notes,
                        ParentTask = isNewTask && moveToDifferentTaskList ?
                            subTask.ParentTask :
                            CurrentTask.TaskID,
                        Position = lastStID,
                        Status = subTask.Status,
                        Title = subTask.Title,
                        ToBeCompletedOn = subTask.ToBeCompletedOn,
                        ToBeSynced = true,
                        UpdatedAt = DateTime.Now
                    };

                    var response = await _dataService
                        .TaskService
                        .AddAsync(taskListID, entity);

                    if (response.Succeed)
                        currentSubTasks.Add(_mapper.Map<TaskItemViewModel>(response.Result));
                }
            }
            ShowTaskProgressRing = false;

            return currentSubTasks;
        }

        private async Task DeleteSubTaskAsync(TaskItemViewModel subTask)
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

            var deleteResponse = await _dataService
                .TaskService
                .RemoveTaskAsync(subTask.TaskID);

            ShowTaskProgressRing = false;
            if (!deleteResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't delete the selected sub task. Error = {deleteResponse.Message}");
                return;
            }
            CurrentTask.SubTasks?.Remove(subTask);
            _messenger.Send(
                new KeyValuePair<string, string>(CurrentTask.TaskID, subTask.TaskID),
                $"{MessageType.SUBTASK_DELETED_FROM_PANE_FRAME}");
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
