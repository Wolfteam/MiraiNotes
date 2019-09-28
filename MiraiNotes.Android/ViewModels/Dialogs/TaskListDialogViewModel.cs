using AutoMapper;
using FluentValidation;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Extensions;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
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
    public class TaskListDialogViewModel : BaseViewModel
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

        public TaskListDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            IMiraiNotesDataService dataService,
            IDialogService dialogService,
            IMapper mapper,
            IValidatorFactory validatorFactory)
            : base(textProvider, messenger, logger.ForContext<TaskListDialogViewModel>(), navigationService, appSettings)
        {
            _dataService = dataService;
            _dialogService = dialogService;
            _mapper = mapper;
            _validator = validatorFactory.GetValidator<TaskListDialogViewModel>();
        }

        public override void SetCommands()
        {
            base.SetCommands();
            AddTaskListCommand = new MvxAsyncCommand(SaveTaskList);
            CloseCommand = new MvxAsyncCommand(() => NavigationService.Close(this));
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
                Title = TaskListTitle,
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
                Logger.Error(
                    $"{nameof(SaveTaskList)} An error occurred while trying to " +
                    $"save tasklist = {TaskListTitle} into db. Error = {response.Message}");
            }

            string msg = response.Succeed
                ? GetText("TaskListWasCreated")
                : GetText("DatabaseUnknownError");

            _dialogService.ShowSnackBar(msg);

            var taskList = _mapper.Map<TaskListItemViewModel>(response.Result);

            Messenger.Publish(new TaskListSavedMsg(this, taskList));
            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            await NavigationService.Close(this);
        }

        private void Validate()
        {
            Errors.Clear();
            var validationResult = _validator.Validate(this);
            Errors.AddRange(validationResult.ToDictionary());
            RaisePropertyChanged(() => IsSaveButtonEnabled);
        }
    }
}