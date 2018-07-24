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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiraiNotes.UWP.ViewModels
{
    public class NewTaskViewModel : ViewModelBase
    {
        #region Members
        private readonly ICustomDialogService _dialogService;
        private readonly IMessenger _messenger;
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IMapper _mapper;

        private string _taskOperationTitle;
        private TaskListModel _currentTaskList;
        private TaskModel _currentTask;
        private DateTimeOffset _minDate = DateTime.Now;
        private bool _showTaskProgressRing;
        private bool _isCurrentTaskTitleFocused;


        #endregion

        #region Properties
        public string TaskOperationTitle
        {
            get { return _taskOperationTitle; }
            set { Set(ref _taskOperationTitle, value); }
        }

        public TaskModel CurrentTask
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
        #endregion

        #region Commands
        public ICommand SaveChangesCommand { get; set; }

        public ICommand DeleteTaskCommand { get; set; }

        public ICommand MarkAsCompletedCommand { get; set; }

        public ICommand ClosePaneCommand { get; set; }
        #endregion

        #region Constructor
        public NewTaskViewModel(
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


            _messenger.Register<TaskListModel>(this, "OnNavigationViewSelectionChange", (taskList) =>
            {
                _currentTaskList = taskList;
                _messenger.Send(false, "OpenPane");
            });
            _messenger.Register<TaskModel>(this, "NewTask", (task) => InitView(task));
            _messenger.Register<string>(this, "OnTaskRemoved", OnTaskRemoved);
            _messenger.Register<bool>(this, "ShowTaskProgressRing", (show) => ShowTaskProgressRing = show);

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
        public void InitView(TaskModel task)
        {
            CurrentTask = new TaskModel
            {
                TaskID = task.TaskID,
                Title = string.IsNullOrEmpty(task.Title) ? "Task title" : task.Title,
                Notes = string.IsNullOrEmpty(task.Notes) ?  "Task body" : task.Notes,
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
                    var u = i as TaskModel;
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
            IsCurrentTaskTitleFocused = true;
            CurrentTask.Validate();
        }

        private async Task SaveChangesAsync()
        {
            var task = _mapper.Map<GoogleTaskModel>(CurrentTask);
            task.UpdatedAt = DateTime.Now;
            bool isNewTask = string.IsNullOrEmpty(task.TaskID);
            if (isNewTask)
                task.Status = GoogleTaskStatus.NEEDS_ACTION.GetString();

            GoogleResponseModel<GoogleTaskModel> response;
            ShowTaskProgressRing = true;
            if (isNewTask)
            {
                response = await _googleApiService.TaskService.SaveAsync(_currentTaskList.TaskListID, task);
            }
            else
            {
                response = await _googleApiService.TaskService.UpdateAsync(_currentTaskList.TaskListID, task.TaskID, task);
            }
            ShowTaskProgressRing = false;

            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    $"An error occurred while trying to {(string.IsNullOrEmpty(task.TaskID) ? "save" : "update")} the task.",
                    $"Status Code: {response.Errors.ApiError.Code}. {response.Errors.ApiError.Message}");
                return;
            }

            CurrentTask = _mapper.Map<TaskModel>(response.Result);
            _messenger.Send(CurrentTask, "TaskSaved");
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

            _messenger.Send(CurrentTask.TaskID, "TaskDeleted");
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
            CurrentTask = new TaskModel
            {
                Title = string.Empty,
                Notes = string.Empty
            };
            _messenger.Send(false, "OpenPane");
        }

        private void UpdateTaskOperationTitle(bool isNewTask)
        {
            if (CurrentTask.IsNew)
                TaskOperationTitle = "New Task:";
            else
                TaskOperationTitle = "Update task:";
        }

        private void OnTaskRemoved(string taskID)
        {
            if (CurrentTask?.TaskID == taskID)
            {
                CleanPanel();
            }
        }
        #endregion
    }
}
