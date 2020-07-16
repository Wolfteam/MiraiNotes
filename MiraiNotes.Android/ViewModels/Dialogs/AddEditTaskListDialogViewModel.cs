using AutoMapper;
using FluentValidation;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Extensions;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Results;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class AddEditTaskListDialogViewModel : BaseViewModel<TaskListItemViewModel, AddEditTaskListDialogViewModelResult>
    {
        #region Members
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;
        private readonly IMapper _mapper;
        private readonly IValidator _validator;

        private string _taskListTitle;
        private ObservableDictionary<string, string> _errors = new ObservableDictionary<string, string>();
        #endregion

        #region Properties
        public bool CreateTaskList
            => Parameter == null;

        public string TaskListTitle
        {
            get => _taskListTitle;
            set
            {
                SetProperty(ref _taskListTitle, value);
                Validate();
            }
        }

        public bool IsSaveButtonEnabled
            => Errors.Count == 0;

        public ObservableDictionary<string, string> Errors
        {
            get => _errors;
            set => SetProperty(ref _errors, value);
        }

        #endregion

        #region Commands
        public IMvxAsyncCommand AddTaskListCommand { get; private set; }
        public IMvxAsyncCommand CloseCommand { get; private set; }
        #endregion

        public AddEditTaskListDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService,
            IMiraiNotesDataService dataService,
            IDialogService dialogService,
            IMapper mapper,
            IValidatorFactory validatorFactory)
            : base(textProvider, messenger, logger.ForContext<AddEditTaskListDialogViewModel>(), navigationService, appSettings, telemetryService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
            _mapper = mapper;
            _validator = validatorFactory.GetValidator<AddEditTaskListDialogViewModel>();
        }

        public override void Prepare(TaskListItemViewModel parameter)
        {
            base.Prepare(parameter);
            if (!CreateTaskList)
            {
                TaskListTitle = parameter.Title;
            }
        }

        public override void SetCommands()
        {
            base.SetCommands();
            AddTaskListCommand = new MvxAsyncCommand(() =>
            {
                if (CreateTaskList)
                    return SaveTaskList();
                return UpdateTaskList(Parameter);
            });
            CloseCommand = new MvxAsyncCommand(() =>
            {
                var result = AddEditTaskListDialogViewModelResult.Nothing();
                return NavigationService.Close(this, result);
            });
        }

        public async Task SaveTaskList()
        {
            Validate();

            if (!IsSaveButtonEnabled)
                return;

            Messenger.Publish(new ShowProgressOverlayMsg(this, true));
            var entity = new GoogleTaskList
            {
                GoogleTaskListID = Guid.NewGuid().ToString(),
                Title = TaskListTitle.Trim(),
                UpdatedAt = DateTimeOffset.UtcNow,
                LocalStatus = LocalStatus.CREATED,
                ToBeSynced = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
            var response = await _dataService
                .TaskListService
                .AddAsync(entity);

            if (!response.Succeed)
            {
                Messenger.Publish(new ShowProgressOverlayMsg(this, false));
                Logger.Error(
                    $"{nameof(SaveTaskList)} An error occurred while trying to " +
                    $"save tasklist = {TaskListTitle} into db. Error = {response.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"), true);
                return;
            }

            _dialogService.ShowSucceedToast(GetText("TaskListWasCreated"));

            var taskList = _mapper.Map<TaskListItemViewModel>(response.Result);

            Messenger.Publish(new TaskListSavedMsg(this, taskList, false));
            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            var result = AddEditTaskListDialogViewModelResult.Created(taskList);

            await NavigationService.Close(this, result);
        }

        public async Task UpdateTaskList(TaskListItemViewModel taskList)
        {
            Validate();

            if (!IsSaveButtonEnabled)
                return;

            Messenger.Publish(new ShowProgressOverlayMsg(this));

            var dbResponse = await _dataService
                .TaskListService
                .FirstOrDefaultAsNoTrackingAsync(t => t.GoogleTaskListID == taskList.GoogleId);

            if (!dbResponse.Succeed)
            {
                Messenger.Publish(new ShowProgressOverlayMsg(this, false));
                Logger.Error(
                    $"{nameof(UpdateTaskList)} An error occurred while trying to " +
                    $"get tasklist = {TaskListTitle} from db. Error = {dbResponse.Message}");

                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"), true);
                return;
            }

            dbResponse.Result.Title = TaskListTitle.Trim();
            dbResponse.Result.UpdatedAt = DateTimeOffset.UtcNow;
            dbResponse.Result.ToBeSynced = true;
            if (dbResponse.Result.LocalStatus != LocalStatus.CREATED)
                dbResponse.Result.LocalStatus = LocalStatus.UPDATED;

            var response = await _dataService
                .TaskListService
                .UpdateAsync(dbResponse.Result);

            if (!response.Succeed)
            {
                Messenger.Publish(new ShowProgressOverlayMsg(this, false));
                Logger.Error(
                    $"{nameof(UpdateTaskList)} An error occurred while trying to " +
                    $"get tasklist = {TaskListTitle} from db. Error = {response.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"), true);
                return;
            }

            _dialogService.ShowSucceedToast(GetText("TaskListWasUpdated"));

            var updatedTaskList = _mapper.Map<TaskListItemViewModel>(dbResponse.Result);

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));
            Messenger.Publish(new TaskListSavedMsg(this, updatedTaskList, true));

            var result = AddEditTaskListDialogViewModelResult.Updated(updatedTaskList);
            await NavigationService.Close(this, result);
        }

        private void Validate()
        {
            Errors.Clear();
            var validationContext = new ValidationContext<AddEditTaskListDialogViewModel>(this);
            var validationResult = _validator.Validate(validationContext);
            Errors.AddRange(validationResult.ToDictionary());
            RaisePropertyChanged(() => IsSaveButtonEnabled);
        }
    }
}