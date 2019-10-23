using AutoMapper;
using FluentValidation;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Extensions;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Android.ViewModels.Dialogs;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MiraiNotes.Shared.Extensions;
using MiraiNotes.Shared.Helpers;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels
{
    public class NewTaskViewModel : BaseViewModel<NewTaskViewModelParameter>
    {
        #region Members
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IValidator _validator;

        private TaskListItemViewModel _currentTaskList;
        private string _selectedTaskId;
        private bool _showProgressBar;
        private TaskItemViewModel _task = Mvx.IoCProvider.Resolve<TaskItemViewModel>();
        private MvxObservableCollection<TaskListItemViewModel> _taskLists = new MvxObservableCollection<TaskListItemViewModel>();
        private TaskListItemViewModel _selectedTaskList;

        private ObservableDictionary<string, string> _errors = new ObservableDictionary<string, string>();
        private List<string> _changedProperties = new List<string>();
        private readonly MvxInteraction _viewModelLoaded = new MvxInteraction();

        public readonly Dictionary<string, string> InitialValues = new Dictionary<string, string>();
        #endregion

        #region Interactors
        public IMvxInteraction ViewModelLoaded
            => _viewModelLoaded;
        #endregion

        #region Properties
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

        public ObservableDictionary<string, string> Errors
        {
            get => _errors;
            set => SetProperty(ref _errors, value);
        }
        #endregion

        #region Commands
        public IMvxAsyncCommand SaveChangesCommand { get; private set; }
        public IMvxAsyncCommand CloseCommand { get; private set; }
        public IMvxAsyncCommand ChangeTaskStatusCommand { get; private set; }
        public IMvxAsyncCommand DeleteTaskCommand { get; private set; }
        public IMvxAsyncCommand AddReminderCommand { get; private set; }
        public IMvxAsyncCommand AddSubTaskCommand { get; private set; }
        public IMvxAsyncCommand MoveTaskCommand { get; private set; }
        public IMvxAsyncCommand AddCompletitionDateCommand { get; private set; }
        #endregion

        public NewTaskViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IMapper mapper,
            IDialogService dialogService,
            IMiraiNotesDataService dataService,
            IAppSettingsService appSettings,
            IValidatorFactory validatorFactory,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<NewTaskViewModel>(), navigationService, appSettings, telemetryService)
        {
            _mapper = mapper;
            _dialogService = dialogService;
            _dataService = dataService;
            _validator = validatorFactory.GetValidator<TaskItemViewModel>();
        }

        public override void Prepare(NewTaskViewModelParameter parameter)
        {
            base.Prepare(parameter);

            _currentTaskList = parameter.TaskList;
            _selectedTaskId = parameter.TaskId;

            Title = string.IsNullOrEmpty(_selectedTaskId)
                ? GetText("NewTask")
                : GetText("UpdateTask");
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            await LoadTaskLists();

            await InitView(_selectedTaskId);
        }

        public override void SetCommands()
        {
            base.SetCommands();
            SaveChangesCommand = new MvxAsyncCommand(SaveChanges);

            CloseCommand = new MvxAsyncCommand(async () =>
            {
                Messenger.Publish(new HideKeyboardMsg(this));

                if (!AppSettings.AskBeforeDiscardChanges || !ChangesWereMade())
                {
                    await NavigationService.Close(this);
                    return;
                }

                bool close = await NavigationService
                    .Navigate<AskBeforeDiscardChangesDialogViewModel, TaskItemViewModel, bool>(Task);

                if (close)
                    await NavigationService.Close(this);
            });

            DeleteTaskCommand = new MvxAsyncCommand(async () =>
            {
                Messenger.Publish(new HideKeyboardMsg(this));
                ShowProgressBar = true;

                bool deleted = await NavigationService
                    .Navigate<DeleteTaskDialogViewModel, TaskItemViewModel, bool>(Task);

                ShowProgressBar = false;
            });

            ChangeTaskStatusCommand = new MvxAsyncCommand(async () =>
            {
                ShowProgressBar = true;

                await NavigationService
                    .Navigate<ChangeTaskStatusDialogViewModel, TaskItemViewModel, bool>(Task);

                ShowProgressBar = false;
            });

            AddReminderCommand = new MvxAsyncCommand(async () =>
            {
                var parameter = TaskDateViewModelParameter.Instance(_currentTaskList, Task, TaskNotificationDateType.REMINDER_DATE, true);
                await NavigationService.Navigate<TaskDateDialogViewModel, TaskDateViewModelParameter, bool>(parameter);
            });

            AddSubTaskCommand = new MvxAsyncCommand(async () =>
            {
                var parameter = AddSubTaskDialogViewModelParameter.Instance(_currentTaskList.GoogleId, Task, true);
                await NavigationService.Navigate<AddSubTaskDialogViewModel, AddSubTaskDialogViewModelParameter, bool>(parameter);
            });

            MoveTaskCommand = new MvxAsyncCommand(async () =>
            {
                var parameter = MoveToTaskListDialogViewModelParameter.Instance(_currentTaskList, Task);
                await NavigationService.Navigate<MoveToTaskListDialogViewModel, MoveToTaskListDialogViewModelParameter, bool>(parameter);
            });

            AddCompletitionDateCommand = new MvxAsyncCommand(async () =>
            {
                var parameter = TaskDateViewModelParameter.Instance(_currentTaskList, Task, TaskNotificationDateType.TO_BE_COMPLETED_DATE, true);
                await NavigationService.Navigate<TaskDateDialogViewModel, TaskDateViewModelParameter, bool>(parameter);
            });
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
            var tokens = new[]
            {
                Messenger.Subscribe<TaskStatusChangedMsg>(OnTaskStatusChanged),
                Messenger.Subscribe<TaskDeletedMsg>(async msg => await OnTaskDeleted(msg.TaskId, msg.HasParentTask)),
                Messenger.Subscribe<TaskMovedMsg>(async msg => await OnTaskDeleted(msg.TaskId, msg.HasParentTask)),
            };

            SubscriptionTokens.AddRange(tokens);
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
                Logger.Error(
                    $"{nameof(LoadTaskLists)}: An error occurred while trying to retrieve all the task lists. " +
                    $"Error = {dbResponse.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                return;
            }

            TaskLists = _mapper.Map<MvxObservableCollection<TaskListItemViewModel>>(dbResponse.Result);

            SelectedTaskList = TaskLists
                .FirstOrDefault(t => t.GoogleId == _currentTaskList.GoogleId);
            ShowProgressBar = false;
        }

        public async Task InitView(string taskId)
        {
            if (string.IsNullOrEmpty(taskId))
            {
                SaveInitialValues();
                return;
            }

            ShowProgressBar = true;
            var tasksResponse = await _dataService
                .TaskService
                .GetAsNoTrackingAsync(t => t.GoogleTaskID == taskId || t.ParentTask == taskId);

            if (!tasksResponse.Succeed)
            {
                ShowProgressBar = false;

                Logger.Error(
                    $"{nameof(InitView)}: An error occurred while trying to retrieve taskId = {taskId}. " +
                    $"Error = {tasksResponse.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                return;
            }

            var task = _mapper.Map<TaskItemViewModel>(tasksResponse.Result.First(t => t.GoogleTaskID == taskId));
            var subTasks = tasksResponse.Result
                .Where(t => t.ParentTask == taskId)
                .OrderBy(t => t.Position);

            task.SubTasks = _mapper.Map<MvxObservableCollection<TaskItemViewModel>>(subTasks);
            Task = task;

            SaveInitialValues();

            ShowProgressBar = false;
        }

        private async Task SaveChanges()
        {
            Messenger.Publish(new HideKeyboardMsg(this));
            bool isNewTask = Task.IsNew;

            Validate();

            if (Errors.Any())
                return;

            if (SelectedTaskList?.GoogleId == null || _currentTaskList?.GoogleId == null)
            {
                Logger.Error(
                    $"{nameof(SaveChanges)}: An error occurred while trying to { (isNewTask ? "save" : "update")} " +
                    $"this task. The selected task list and the current task list cant be null");
                _dialogService.ShowErrorToast(GetText("UnknownErrorOccurred"));
                return;
            }

            //If the task list selected in the combo is not the same as the one in the 
            //navigation view, its because we are trying to save/update a 
            //task into a different task list
            bool moveToDifferentTaskList = SelectedTaskList.GoogleId != _currentTaskList.GoogleId;

            //If we are updating a task but also moving it into a different tasklist
            if (moveToDifferentTaskList && !isNewTask)
            {
                throw new Exception("Moving an existing task is not possible while saving changes");
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
                    .FirstOrDefaultAsNoTrackingAsync(t => t.GoogleTaskID == Task.GoogleId);

                if (!dbResponse.Succeed || dbResponse.Result == null)
                {
                    ShowProgressBar = false;
                    Logger.Error(
                        $"{nameof(SaveChanges)}: Couldnt find the task to update. TaskId ={Task.GoogleId}. " +
                        $"Error = {dbResponse.Message}");
                    _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                    return;
                }

                entity = dbResponse.Result;
            }

            if (isNewTask)
                entity.CreatedAt = DateTimeOffset.UtcNow;
            entity.CompletedOn = Task.CompletedOn;
            entity.GoogleTaskID = Task.IsNew
                ? Guid.NewGuid().ToString()
                : Task.GoogleId;
            entity.IsDeleted = Task.IsDeleted;
            entity.IsHidden = Task.IsHidden;
            entity.Notes = Task.Notes;
            entity.ParentTask = Task.ParentTask;
            entity.Position = isNewTask
                ? null
                : entity.Position;
            entity.Status = Task.IsNew
                ? GoogleTaskStatus.NEEDS_ACTION.GetString()
                : Task.Status;
            entity.Title = Task.Title.Trim();
            entity.LocalStatus = Task.IsNew
                ? LocalStatus.CREATED
                : entity.LocalStatus == LocalStatus.CREATED
                    ? LocalStatus.CREATED
                    : LocalStatus.UPDATED;
            entity.ToBeSynced = true;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            entity.ToBeCompletedOn = Task.ToBeCompletedOn;
            entity.RemindOn = Task.RemindOn;
            entity.RemindOnGUID = Task.RemindOnGUID;

            ResponseDto<GoogleTask> response;

            //If we are creating a new task but in a different tasklist
            if (moveToDifferentTaskList)
            {
                await SaveNewTaskIntoDifferentTaskList(entity);
                return;
            }

            if (isNewTask)
            {
                response = await _dataService
                    .TaskService
                    .AddAsync(_currentTaskList.GoogleId, entity);
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
                Logger.Error(
                    $"{nameof(SaveChanges)}: An error occurred while trying to {(isNewTask ? "save" : "update")} the task." +
                    $"Error = {response.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                return;
            }

            //this 2 lines must run before we reset the Task property
            var subTasksToSave = GetSubTasksToSave(isNewTask, false);
            var currentSts = GetCurrentSubTasks();

            Task = _mapper.Map<TaskItemViewModel>(response.Result);

            var sts = await SaveSubTasksAsync(subTasksToSave, isNewTask, false, currentSts);

            Task.SubTasks = new MvxObservableCollection<TaskItemViewModel>(sts);

            Messenger.Publish(new TaskSavedMsg(this, Task.GoogleId, 1 + subTasksToSave.Count));

            await NavigationService.Close(this);
        }

        private async Task SaveNewTaskIntoDifferentTaskList(GoogleTask entity)
        {
            var response = await _dataService
                .TaskService
                .AddAsync(SelectedTaskList.GoogleId, entity);

            if (!response.Succeed)
            {
                ShowProgressBar = false;
                Logger.Error(
                    $"{nameof(SaveChanges)}: An error occurred while trying to seve the task into {SelectedTaskList.Title}." +
                    $"Error = {response.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                return;
            }
            var subTasksToSave = GetSubTasksToSave(true, true);

            subTasksToSave.ForEach(st => st.ParentTask = entity.GoogleTaskID);

            await SaveSubTasksAsync(
                subTasksToSave,
                true,
                true,
                Enumerable.Empty<TaskItemViewModel>().ToList());

            _dialogService.ShowSnackBar(GetText("TaskWasCreated", SelectedTaskList.Title));

            //TODO: I SHOULD DO SOMETHING HERE WHEN MOVING THE TASK
            await NavigationService.Close(this);
        }


        private async Task<IEnumerable<TaskItemViewModel>> SaveSubTasksAsync(
            IEnumerable<TaskItemViewModel> subTasksToSave,
            bool isNewTask,
            bool moveToDifferentTaskList,
            List<TaskItemViewModel> currentSubTasks)
        {
            ShowProgressBar = true;
            string taskListId = moveToDifferentTaskList
                ? SelectedTaskList.GoogleId
                : _currentTaskList.GoogleId;

            var orderedSubTasks = subTasksToSave.OrderBy(st => st.CreatedAt).ToList();
            foreach (var subTask in orderedSubTasks)
            {
                string parentTask = isNewTask && moveToDifferentTaskList
                        ? subTask.ParentTask
                        : Task.GoogleId;

                var entity = new GoogleTask
                {
                    CompletedOn = subTask.CompletedOn,
                    CreatedAt = subTask.CreatedAt,
                    GoogleTaskID = subTask.IsNew
                        ? Guid.NewGuid().ToString()
                        : subTask.GoogleId,
                    IsDeleted = subTask.IsDeleted,
                    IsHidden = subTask.IsHidden,
                    LocalStatus = LocalStatus.CREATED,
                    Notes = subTask.Notes,
                    ParentTask = parentTask,
                    Status = subTask.Status,
                    Title = subTask.Title.Trim(),
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

            ShowProgressBar = false;

            return currentSubTasks;
        }

        private List<TaskItemViewModel> GetSubTasksToSave(bool isCurrentTaskNew, bool moveToDifferentTaskList)
        {
            //if the current task is new or we are not moving it to
            //a different task list, we choose the st that are new and not completed
            if (isCurrentTaskNew || !moveToDifferentTaskList)
                return Task.SubTasks
                           .Where(st => st.IsNew && st.CompletedOn == null)
                           .ToList();
            //else, the current task is not new and we are moving it to
            //a different task list, so we choose all the st
            return Task.SubTasks.ToList();
        }

        private List<TaskItemViewModel> GetCurrentSubTasks()
        {
            return Task.SubTasks
                       .Where(st => !st.IsNew)
                       .OrderBy(st => st.CreatedAt)
                       .ToList();
        }

        private void OnTaskStatusChanged(TaskStatusChangedMsg msg)
        {
            TaskItemViewModel taskFound = null;

            if (!msg.HasParentTask)
            {
                taskFound = Task?.GoogleId == msg.TaskId
                    ? Task
                    : null;
            }
            else
            {
                taskFound = Task?
                    .SubTasks?
                    .FirstOrDefault(st => st.GoogleId == msg.TaskId);
            }

            if (taskFound == null)
                return;

            taskFound.CompletedOn = msg.CompletedOn;
            taskFound.UpdatedAt = msg.UpdatedAt;
            taskFound.Status = msg.NewStatus;
        }

        private async Task OnTaskDeleted(string taskId, bool hasParentTask)
        {
            //if the deleted task isnt a sub task, just return
            if (!hasParentTask)
            {
                await CloseCommand.ExecuteAsync();
                return;
            }

            Task.SubTasks.RemoveAll(st => st.GoogleId == taskId);
        }

        private void Validate()
        {
            Errors.Clear();
            var validationResult = _validator.Validate(Task);
            Errors.AddRange(validationResult.ToDictionary());
        }

        private void SaveInitialValues()
        {
            var propertyTypes = new[] { typeof(string), typeof(bool), typeof(DateTimeOffset) };
            var properties = Task.GetType()
                .GetProperties()
                .Where(p =>
                    p.CanWrite &&
                    p.CanRead &&
                    p.MemberType == MemberTypes.Property &&
                    propertyTypes.Contains(p.PropertyType));

            foreach (PropertyInfo pi in properties)
            {
                InitialValues[pi.Name] = pi.GetValue(Task, null)?.ToString();
            }
        }

        private void PropertyIsDirty(string property, bool isDirty)
        {
            if (isDirty && !_changedProperties.Contains(property))
            {
                _changedProperties.Add(property);
            }
            else if (!isDirty && _changedProperties.Contains(property))
            {
                _changedProperties.Remove(property);
            }
        }

        public void TextChanged(string property, string newValue)
        {
            if (InitialValues.ContainsKey(property) &&
                InitialValues[property] != newValue)
            {
                PropertyIsDirty(property, true);
            }
            else
            {
                PropertyIsDirty(property, false);
            }
        }

        public bool ChangesWereMade() =>
            _changedProperties.Any();
    }
}