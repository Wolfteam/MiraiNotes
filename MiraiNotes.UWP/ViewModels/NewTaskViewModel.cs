using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
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

        private TaskModel _currentTask;
        private DateTimeOffset _minDate = DateTime.Now;
        private bool _showTaskProgressRing;
        private bool _isCurrentTaskTitleFocused;


        #endregion

        #region Properties
        public GoogleTaskListModel CurrentTaskList { get; set; }

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

            SaveChangesCommand = new RelayCommand(async () => await SaveChangesAsync());
            MarkAsCompletedCommand = new RelayCommand(async () => await MarkAsCompletedAsync());
            ClosePaneCommand = new RelayCommand(CleanPanel);

            _messenger.Register<GoogleTaskListModel>(this, "OnNavigationViewSelectionChange", 
                (taskList) => CurrentTaskList = taskList);
            _messenger.Register<TaskModel>(this, "NewTask", (task) => InitView(task));
        }
        #endregion

        #region Methods
        public void InitView(TaskModel task)
        {
            CurrentTask = new TaskModel
            {
                TaskID = task.TaskID,
                Title = task.Title,
                Notes = task.Notes,
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
                    //TODO: Validation is not working
                    var u = i as TaskModel;
                    if (string.IsNullOrEmpty(u.Title))
                    {
                        u.Properties[nameof(u.Title)].Errors.Add("Title is required");
                    }
                    if (string.IsNullOrEmpty(u.Notes))
                    {
                        u.Properties[nameof(u.Notes)].Errors.Add("Notes are required");
                    }
                }
            };
            IsCurrentTaskTitleFocused = true;
        }

        public async Task MarkAsCompletedAsync()
        {
            CurrentTask.Status = "completed";
            await SaveChangesAsync();
        }

        private async Task SaveChangesAsync()
        {
            bool isModelValid = CurrentTask.Validate();
            if (!isModelValid)
            {
                await _dialogService.ShowMessageDialogAsync("Error", "Faltan campos");
                return;
            }
            var task = _mapper.Map<GoogleTaskModel>(CurrentTask);
            task.UpdatedAt = DateTime.Now;
            bool isNewTask = string.IsNullOrEmpty(task.TaskID);
            if (isNewTask)
                task.Status = "needsAction";

            GoogleResponseModel<GoogleTaskModel> response;
            ShowTaskProgressRing = true;
            if (isNewTask)
            {
                response = await _googleApiService.TaskService.SaveAsync(CurrentTaskList.TaskListID, task);
            }
            else
            {
                response = await _googleApiService.TaskService.UpdateAsync(CurrentTaskList.TaskListID, task.TaskID, task);
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
        }

        private void CleanPanel()
        {
            CurrentTask.Title = string.Empty;
            CurrentTask.Notes = string.Empty;
            _messenger.Send(false, "OpenPane");
        }
        #endregion
    }
}
