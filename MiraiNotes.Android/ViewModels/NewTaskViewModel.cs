using AutoMapper;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MiraiNotes.Shared.Helpers;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels
{
    public class NewTaskViewModel : BaseViewModel<Tuple<TaskListItemViewModel, string>>
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IAppSettingsService _appSettings;
        private readonly IGoogleApiService _googleApiService;
        private readonly INotificationService _notificationService;
        private readonly IUserCredentialService _userCredentialService;

        private TaskListItemViewModel _currentTaskList;
        private string _selectedTaskId;
        private bool _showProgressBar;
        private TaskItemViewModel _task;
        private MvxObservableCollection<TaskListItemViewModel> _taskLists = new MvxObservableCollection<TaskListItemViewModel>();
        private TaskListItemViewModel _selectedTaskList;

        public bool ShowProgressBar
        {
            get => _showProgressBar;
            set => SetProperty(ref _showProgressBar, value);
        }

        public TaskItemViewModel Task
        {
            get => _task;
            set => SetProperty(ref _task, value);
        }

        public MvxObservableCollection<TaskListItemViewModel> TaskLists
        {
            get => _taskLists;
            set => SetProperty(ref _taskLists, value);
        }

        public TaskListItemViewModel SelectedTaskList
        {
            get => _selectedTaskList;
            set => SetProperty(ref _selectedTaskList, value);
        }


        public IMvxAsyncCommand SaveChangesCommand { get; private set; }
        public IMvxAsyncCommand CloseCommand { get; private set; }
        public IMvxAsyncCommand ChangeTaskStatusCommand { get; private set; }
        public IMvxCommand DeleteTaskCommand { get; private set; }

        public NewTaskViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            IMapper mapper,
            IDialogService dialogService,
            IMiraiNotesDataService dataService,
            IAppSettingsService appSettings,
            IGoogleApiService googleApiService,
            INotificationService notificationService,
            IUserCredentialService userCredentialService)
            : base(textProvider, messenger)
        {
            _navigationService = navigationService;
            _mapper = mapper;
            _dialogService = dialogService;
            _dataService = dataService;
            _appSettings = appSettings;
            _googleApiService = googleApiService;
            _notificationService = notificationService;
            _userCredentialService = userCredentialService;

            SetCommands();
        }

        public override void Prepare(Tuple<TaskListItemViewModel, string> kvp)
        {
            _currentTaskList = kvp.Item1;
            _selectedTaskId = kvp.Item2;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await LoadTaskLists();

            await InitView(_selectedTaskId);
        }

        private void SetCommands()
        {
            SaveChangesCommand = new MvxAsyncCommand(SaveChanges);

            CloseCommand = new MvxAsyncCommand(
                async () => await _navigationService.Close(this));

            DeleteTaskCommand = new MvxCommand(() => _dialogService.ShowDialog(
                "Confirmation",
                "Are you sure you wanna delete this task?",
                "Yes",
                "No",
                async () => await DeleteTask())
            );
        }

        private async Task LoadTaskLists()
        {
            ShowProgressBar = true;

            var dbResponse = await _dataService
                .TaskListService
                .GetAsNoTrackingAsync(
                    tl => tl.LocalStatus != LocalStatus.DELETED &&
                          tl.User.IsActive,
                    tl => tl.OrderBy(t => t.Title));

            if (!dbResponse.Succeed)
            {
                ShowProgressBar = false;
                _dialogService.ShowErrorToast(
                    $"An error occurred while trying to retrieve all the task lists. Error = {dbResponse.Message}");
                return;
            }

            SelectedTaskList = TaskLists
                .FirstOrDefault(t => t.Id == _currentTaskList.Id);
            ShowProgressBar = false;
        }

        public async Task InitView(string taskId)
        {
            //MinDate = DateTimeOffset.Now;

            ShowProgressBar = true;
            if (string.IsNullOrEmpty(taskId))
            {
                Task = new TaskItemViewModel();
            }
            else
            {
                var ta = await _dataService
                    .TaskService
                    .FirstOrDefaultAsNoTrackingAsync(x => x.GoogleTaskID == taskId);

                var sts = await _dataService
                    .TaskService
                    .GetAsNoTrackingAsync(
                        st => st.ParentTask == taskId,
                        st => st.OrderBy(x => x.Position));

                if (!ta.Succeed || !sts.Succeed)
                {
                    ShowProgressBar = false;
                    _dialogService.ShowErrorToast(
                        $"An unexpected error occurred. Error = {ta.Message} {sts.Message}");
                    return;
                }

                var t = _mapper.Map<TaskItemViewModel>(ta.Result);
                t.SubTasks = _mapper.Map<MvxObservableCollection<TaskItemViewModel>>(sts.Result);
                Task = t;
            }

            ShowProgressBar = false;
        }

        private async Task SaveChanges()
        {
            bool isNewTask = Task.IsNew;

            if (SelectedTaskList?.Id == null || _currentTaskList?.Id == null)
            {
                _dialogService.ShowSnackBar(
                    $"An error occurred while trying to {(isNewTask ? "save" : "update")} the task." +
                    $"The selected task list and the current task list cant be null", string.Empty);
                return;
            }

            if (Task.RemindOn.HasValue)
            {
                var minutesDiff = (Task.RemindOn.Value - DateTimeOffset.Now).TotalMinutes;
                if (minutesDiff < 2)
                {
                    _dialogService.ShowSnackBar(
                        "The date of the reminder must be at least 2 mins above the current time.",
                        string.Empty);
                    return;
                }
            }

            //If the task list selected in the combo is not the same as the one in the 
            //navigation view, its because we are trying to save/update a 
            //task into a different task list
            bool moveToDifferentTaskList = SelectedTaskList.Id != _currentTaskList.Id;

            //If we are updating a task but also moving it into a different tasklist
            if (moveToDifferentTaskList && !isNewTask)
            {
                _dialogService.ShowDialog(
                    "Confirm",
                    "Since you are moving an existing task to a different task list, any change made here will be lost. Do you want to continue ?",
                    "Yes",
                    "No",
                    async () => await MoveCurrentTask());
                return;
            }

            GoogleTask entity;
            ShowProgressBar = true;

            if (isNewTask)
            {
                entity = new GoogleTask();
            }
            else
            {
                var dbResponse = await _dataService
                    .TaskService
                    .FirstOrDefaultAsNoTrackingAsync(t => t.GoogleTaskID == Task.TaskID);

                if (!dbResponse.Succeed || dbResponse.Result == null)
                {
                    ShowProgressBar = false;
                    _dialogService.ShowErrorToast(
                        $"Couldn't find the task to update from db. Error = {dbResponse.Message}");
                    return;
                }

                entity = dbResponse.Result;
            }

            if (isNewTask)
                entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.CompletedOn = Task.CompletedOn;
            entity.GoogleTaskID = Task.IsNew
                ? Guid.NewGuid().ToString()
                : Task.TaskID;
            entity.IsDeleted = Task.IsDeleted;
            entity.IsHidden = Task.IsHidden;
            entity.Notes = Task.Notes;
            entity.ParentTask = Task.ParentTask;
            entity.Position = Task.Position;
            entity.Status = Task.IsNew
                ? GoogleTaskStatus.NEEDS_ACTION.GetString()
                : Task.Status;
            entity.Title = Task.Title;
            entity.LocalStatus = Task.IsNew
                ? LocalStatus.CREATED
                : entity.LocalStatus == LocalStatus.CREATED
                    ? LocalStatus.CREATED
                    : LocalStatus.UPDATED;
            entity.ToBeSynced = true;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            entity.ToBeCompletedOn = Task.ToBeCompletedOn;
            entity.RemindOn = Task.RemindOn;

            //If the current task has a reminder date and the entity doesn't have
            //a reminder guid, we set it up. I do this to avoid replacing the guid
            if (Task.RemindOn.HasValue && string.IsNullOrEmpty(entity.RemindOnGUID))
            {
                entity.RemindOnGUID = string.Join("", $"{entity.GetHashCode()}".Where(c => c != '-'));
            }

            ResponseDto<GoogleTask> response;
            var subTasksToSave = GetSubTasksToSave(isNewTask, moveToDifferentTaskList);
            var currentSts = GetCurrentSubTasks();
            //If we are creating a new task but in a different tasklist
            if (moveToDifferentTaskList)
            {
                response = await _dataService
                    .TaskService
                    .AddAsync(SelectedTaskList.Id, entity);

                if (!response.Succeed)
                {
                    ShowProgressBar = false;
                    _dialogService.ShowErrorToast(
                        $"An error occurred while trying to seve the task into {SelectedTaskList.Title}." +
                        $"Error = {response.Message}.");
                    return;
                }

                subTasksToSave.ForEach(st => st.ParentTask = entity.GoogleTaskID);

                await SaveSubTasksAsync(
                    subTasksToSave,
                    isNewTask,
                    moveToDifferentTaskList,
                    Enumerable.Empty<TaskItemViewModel>().ToList());

                _dialogService.ShowSnackBar(
                    $"The task was sucessfully created into {SelectedTaskList.Title}",
                    string.Empty);

                //TODO: I SHOULD DO SOMETHING HERE WHEN MOVING THE TASK
                await CloseCommand.ExecuteAsync();
                return;
            }

            if (isNewTask)
            {
                response = await _dataService
                    .TaskService
                    .AddAsync(_currentTaskList.Id, entity);
            }
            else
            {
                response = await _dataService
                    .TaskService
                    .UpdateAsync(entity);
            }

            ShowProgressBar = false;

            if (!response.Succeed)
            {
                _dialogService.ShowErrorToast(
                    $"An error occurred while trying to {(isNewTask ? "save" : "update")} the task." +
                    $"Error: {response.Message}.");
                return;
            }

            Task = _mapper.Map<TaskItemViewModel>(response.Result);

            var sts = await SaveSubTasksAsync(subTasksToSave, isNewTask, moveToDifferentTaskList, currentSts);

            Task.SubTasks = new MvxObservableCollection<TaskItemViewModel>(sts);

            if (Task.RemindOn.HasValue)
            {
                string notes = Task.Notes.Length > 15
                    ? $"{Task.Notes.Substring(0, 15)}...."
                    : $"{Task.Notes}....";

                int id = int.Parse(response.Result.RemindOnGUID);

                _notificationService.RemoveScheduledNotification(id);
                _notificationService.ScheduleNotification(new TaskReminderNotification
                {
                    Id = id,
                    TaskListId = _currentTaskList.Id,
                    TaskId = Task.TaskID,
                    TaskListTitle = _currentTaskList.Title,
                    TaskTitle = Task.Title,
                    TaskBody = notes,
                    DeliveryOn = Task.RemindOn.Value
                });
            }
            Messenger.Publish(new TaskSavedMsg(this, Task.TaskID));

            await CloseCommand.ExecuteAsync();
        }

        private void PromptTaskStatusChange(TaskItemViewModel task, GoogleTaskStatus newStatus)
        {
            string statusMessage =
                $"{(newStatus == GoogleTaskStatus.COMPLETED ? "completed" : "incompleted")}";

            _dialogService.ShowDialog(
                "Confirmation",
                $"Mark {task.Title} as {statusMessage}?",
                "Yes",
                "No",
                async () => await ChangeTaskStatus(task, newStatus));
        }

        private async Task ChangeTaskStatus(TaskItemViewModel task, GoogleTaskStatus newStatus)
        {
            string statusMessage =
                $"{(newStatus == GoogleTaskStatus.COMPLETED ? "completed" : "incompleted")}";

            ShowProgressBar = true;

            var response = await _dataService
                .TaskService
                .ChangeTaskStatusAsync(task.TaskID, newStatus);

            ShowProgressBar = false;

            if (!response.Succeed)
            {
                _dialogService.ShowErrorToast(
                    $"An error occurred while trying to mark {task.Title} as {statusMessage}. " +
                    $"Error = {response.Message}.");
                return;
            }

            task.Status = response.Result.Status;
            task.CompletedOn = response.Result.CompletedOn;
            task.UpdatedAt = response.Result.UpdatedAt;

            Messenger.Publish(new TaskStatusChangedMsg(
                this, 
                task.TaskID, 
                task.ParentTask, 
                task.CompletedOn, 
                task.UpdatedAt, 
                task.Status));


            _dialogService.ShowSnackBar(
                $"{task.Title} was marked as {statusMessage}.",
                string.Empty);
        }

        private async Task DeleteTask()
        {
            ShowProgressBar = true;

            var deleteResponse = await _dataService
                .TaskService
                .RemoveTaskAsync(Task.TaskID);

            ShowProgressBar = false;

            if (!deleteResponse.Succeed)
            {
                _dialogService.ShowErrorToast(
                    $"Couldn't delete the selected task. Error = {deleteResponse.Message}.");
                return;
            }

            Messenger.Publish(new TaskDeletedMsg(this, Task.TaskID, Task.ParentTask));
            await _navigationService.Close(this);
        }

        private async Task MoveCurrentTask()
        {
            ShowProgressBar = true;

            var moveResponse = await _dataService
                .TaskService
                .MoveAsync(SelectedTaskList.Id, Task.TaskID, null, null);

            if (!moveResponse.Succeed)
            {
                ShowProgressBar = false;
                _dialogService.ShowErrorToast(
                    $"An error occurred while trying to move the selected task from {_currentTaskList.Title} to {SelectedTaskList.Title}." +
                    $"Error: {moveResponse.Message}.");
                return;
            }

            Messenger.Publish(new TaskDeletedMsg(this, Task.TaskID, Task.ParentTask));

            var subTasks = GetSubTasksToSave(false, true);

            subTasks.ForEach(st => st.ParentTask = moveResponse.Result.GoogleTaskID);

            ShowProgressBar = false;

            await SaveSubTasksAsync(subTasks, false, true, Enumerable.Empty<TaskItemViewModel>().ToList());

            _dialogService.ShowSnackBar(
                $"Task successfully moved from: {_currentTaskList.Title} to: {SelectedTaskList.Title}",
                string.Empty);

            //TODO: SHOULD I DO SOMETHING HERE WHEN MOVING THE TASK ?
            await CloseCommand.ExecuteAsync();
        }

        private async Task DeleteSubTask(TaskItemViewModel subTask)
        {
            if (subTask.IsNew)
            {
                Task.SubTasks?.Remove(subTask);
                return;
            }

            ShowProgressBar = true;

            var deleteResponse = await _dataService
                .TaskService
                .RemoveTaskAsync(subTask.TaskID);

            ShowProgressBar = false;
            if (!deleteResponse.Succeed)
            {
                _dialogService.ShowErrorToast(
                    $"Couldn't delete the selected sub task. Error = {deleteResponse.Message}");
                return;
            }

            Task.SubTasks?.Remove(subTask);

            Messenger.Publish(new TaskDeletedMsg(this, Task.TaskID, subTask.TaskID));
        }

        private async Task RemoveTaskNotificationDate(TaskNotificationDateType dateType)
        {
            string message = dateType == TaskNotificationDateType.TO_BE_COMPLETED_DATE 
                ? "completition" 
                : "reminder";

            ShowProgressBar = true;
            try
            {
                if (!Task.IsNew)
                {
                    var response = await _dataService
                        .TaskService
                        .RemoveNotificationDate(Task.TaskID, dateType);

                    if (!response.Succeed)
                    {
                        _dialogService.ShowErrorToast(
                            $"Could not remove the {message} date of {Task.Title}");
                        return;
                    }

                    if (dateType == TaskNotificationDateType.REMINDER_DATE)
                    {
                        int id = int.Parse(response.Result.RemindOnGUID);
                        _notificationService.RemoveScheduledNotification(id);
                    }

                    Task = _mapper.Map<TaskItemViewModel>(response.Result);
                }
                else
                {
                    switch (dateType)
                    {
                        case TaskNotificationDateType.TO_BE_COMPLETED_DATE:
                            Task.ToBeCompletedOn = null;
                            break;
                        case TaskNotificationDateType.REMINDER_DATE:
                            Task.RemindOn = null;
                            break;
                    }
                }

                Messenger.Publish(new TaskSavedMsg(this, Task.TaskID));
            }
            finally
            {
                ShowProgressBar = false;
            }
        }

        private async Task<IEnumerable<TaskItemViewModel>> SaveSubTasksAsync(
            IEnumerable<TaskItemViewModel> subTasksToSave,
            bool isNewTask,
            bool moveToDifferentTaskList,
            List<TaskItemViewModel> currentSubTasks)
        {
            ShowProgressBar = true;
            string taskListId = moveToDifferentTaskList
                ? SelectedTaskList.Id
                : _currentTaskList.Id;

            if (moveToDifferentTaskList && !isNewTask)
            {
                foreach (var subTask in subTasksToSave)
                {
                    var lastStId = currentSubTasks.LastOrDefault()?.TaskID;
                    var moveResponse = await _dataService
                        .TaskService
                        .MoveAsync(taskListId, subTask.TaskID, subTask.ParentTask, lastStId);
                    if (moveResponse.Succeed)
                        currentSubTasks.Add(_mapper.Map<TaskItemViewModel>(moveResponse.Result));
                }
            }
            else
            {
                foreach (var subTask in subTasksToSave)
                {
                    var lastStId = currentSubTasks.LastOrDefault()?.TaskID;
                    var entity = new GoogleTask
                    {
                        CompletedOn = subTask.CompletedOn,
                        CreatedAt = DateTimeOffset.UtcNow,
                        GoogleTaskID = subTask.IsNew
                            ? Guid.NewGuid().ToString()
                            : subTask.TaskID,
                        IsDeleted = subTask.IsDeleted,
                        IsHidden = subTask.IsHidden,
                        LocalStatus = LocalStatus.CREATED,
                        Notes = subTask.Notes,
                        ParentTask = isNewTask && moveToDifferentTaskList
                            ? subTask.ParentTask
                            : Task.TaskID,
                        Position = lastStId,
                        Status = subTask.Status,
                        Title = subTask.Title,
                        ToBeCompletedOn = subTask.ToBeCompletedOn,
                        ToBeSynced = true,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };

                    var response = await _dataService
                        .TaskService
                        .AddAsync(taskListId, entity);

                    if (response.Succeed)
                        currentSubTasks.Add(_mapper.Map<TaskItemViewModel>(response.Result));
                }
            }

            ShowProgressBar = false;

            return currentSubTasks;
        }

        private List<TaskItemViewModel> GetSubTasksToSave(bool isCurrentTaskNew, bool moveToDifferentTaskList)
        {
            //if the current task is new or we are not moving it to
            //a different task list, we choose the st that are new and not completed
            if (isCurrentTaskNew || !moveToDifferentTaskList)
                return Task.SubTasks?
                           .Where(st => st.IsNew && st.CompletedOn == null)
                           .ToList() ??
                       Enumerable.Empty<TaskItemViewModel>()
                           .ToList();
            //else, the current task is not new and we are moving it to
            //a different task list, so we choose all the st
            return Task.SubTasks?.ToList() ??
                   Enumerable.Empty<TaskItemViewModel>()
                       .ToList();
        }

        private List<TaskItemViewModel> GetCurrentSubTasks()
        {
            return Task.SubTasks?
                       .Where(st => !st.IsNew)
                       .ToList() ??
                   Enumerable.Empty<TaskItemViewModel>()
                       .ToList();
        }
    }
}