﻿using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.UWP.Helpers;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Models.API;
using System;
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
        private bool _isNewTask;
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

        public bool IsNewTask
        {
            get { return _isNewTask; }
            set { Set(ref _isNewTask, value); }
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

            _messenger.Register<TaskListItemViewModel>(this, $"{MessageType.NAVIGATIONVIEW_SELECTION_CHANGED}", (taskList) =>
            {
                _currentTaskList = taskList;
                _messenger.Send(false, $"{MessageType.OPEN_PANE}");
            });
            _messenger.Register<TaskItemViewModel>(this, $"{MessageType.NEW_TASK}", (task) => InitView(task));
            _messenger.Register<string>(this, $"{MessageType.TASK_DELETED_FROM_CONTENT_FRAME}", OnTaskRemoved);
            _messenger.Register<bool>(this, $"{MessageType.SHOW_PANE_FRAME_PROGRESS_RING}", (show) => ShowTaskProgressRing = show);

            SaveChangesCommand = new RelayCommand
                (async () => await SaveChangesAsync());

            DeleteTaskCommand = new RelayCommand
                (async () => await DeleteTask());

            MarkAsCompletedCommand = new RelayCommand
                (async () => await MarkAsCompletedAsync());

            ClosePaneCommand = new RelayCommand(CleanPanel);
        }
        #endregion

        #region Methods
        public async void InitView(TaskItemViewModel task)
        {
            CurrentTask = new TaskItemViewModel
            {
                TaskID = task.TaskID,
                Title = string.IsNullOrEmpty(task.Title) ? "Task title" : task.Title,
                Notes = string.IsNullOrEmpty(task.Notes) ? "Task body" : task.Notes,
                IsNew = string.IsNullOrEmpty(task.TaskID),
                CompletedOn = task.CompletedOn,
                IsDeleted = task.IsDeleted,
                IsHidden = task.IsHidden,
                ParentTask = task.ParentTask,
                Position = task.Position,
                SelfLink = task.SelfLink,
                Status = task.Status,
                ToBeCompletedOn = task.ToBeCompletedOn,
                UpdatedAt = task.UpdatedAt,
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
            IsNewTask = CurrentTask.IsNew;
            //IsCurrentTaskTitleFocused = true;
            //CurrentTask.Validate();

            await GetAllTaskListAsync();
        }

        private async Task SaveChangesAsync()
        {
            var task = _mapper.Map<GoogleTaskModel>(CurrentTask);
            task.UpdatedAt = DateTime.Now;
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
            //If the task selected in the combo is not the same as the one in the 
            //navigation view, its because we are trying to save/update a 
            //task intoto a different task list
            if (SelectedTaskList.TaskListID != _currentTaskList.TaskListID)
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
            _messenger.Send(CurrentTask, $"{MessageType.TASK_SAVED}");
            UpdateTaskOperationTitle(isNewTask);
        }

        public async Task DeleteTask()
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

        public async Task MarkAsCompletedAsync()
        {
            bool markAsCompleted = await _dialogService.ShowConfirmationDialogAsync(
                "Confirmation",
                $"Mark {CurrentTask.Title} as completed?",
                "Yes",
                "No");
            if (!markAsCompleted)
                return;

            CurrentTask.Status = GoogleTaskStatus.COMPLETED.GetString();
            CurrentTask.CompletedOn = DateTime.Now;
            await SaveChangesAsync();
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

            ShowTaskProgressRing = false;

            await _dialogService.ShowMessageDialogAsync(
                "Succeed",
                $"Task sucessfully moved from: {_currentTaskList.Title} to: {SelectedTaskList.Title}");
        }
        #endregion
    }
}