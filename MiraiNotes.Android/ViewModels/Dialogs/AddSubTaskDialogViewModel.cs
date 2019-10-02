using FluentValidation;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Extensions;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using MiraiNotes.Shared.Helpers;
using MiraiNotes.Android.Models.Parameters;
using AutoMapper;
using MvvmCross.Commands;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class AddSubTaskDialogViewModel : BaseConfirmationDialogViewModel<AddSubTaskDialogViewModelParameter, bool>
    {
        private readonly IValidator _validator;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;
        private readonly IMapper _mapper;

        private string _subTaskTitle;
        private ObservableDictionary<string, string> _errors = new ObservableDictionary<string, string>();

        public string SubTaskTitle
        {
            get => _subTaskTitle;
            set
            {
                SetProperty(ref _subTaskTitle, value);
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

        public AddSubTaskDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            IValidatorFactory validatorFactory,
            IMiraiNotesDataService dataService,
            IDialogService dialogService,
            IMapper mapper)
            : base(textProvider, messenger, logger.ForContext<AddSubTaskDialogViewModel>(), navigationService, appSettings)
        {
            _validator = validatorFactory.GetValidator<AddSubTaskDialogViewModel>();
            _dataService = dataService;
            _dialogService = dialogService;
            _mapper = mapper;
        }

        public override void SetCommands()
        {
            base.SetCommands();

            OkCommand = new MvxAsyncCommand(SaveSubTask);
            CloseCommand = new MvxAsyncCommand(() => NavigationService.Close(this, false));
        }


        private async Task SaveSubTask()
        {
            Validate();
            if (!IsSaveButtonEnabled)
                return;

            Messenger.Publish(new ShowProgressOverlayMsg(this));
            var task = Parameter.Task;
            var lastStId = task.SubTasks.OrderBy(st => st.Position).LastOrDefault()?.TaskID;
            var now = DateTimeOffset.UtcNow;
            var entity = new GoogleTask
            {
                CreatedAt = now,
                GoogleTaskID = Guid.NewGuid().ToString(),
                LocalStatus = LocalStatus.CREATED,
                ParentTask = task.TaskID,
                Position = lastStId,
                Status = GoogleTaskStatus.NEEDS_ACTION.GetString(),
                Title = SubTaskTitle.Trim(),
                ToBeSynced = true,
                UpdatedAt = now
            };

            var response = await _dataService
                .TaskService
                .AddAsync(Parameter.TaskListId, entity);

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));
            if (response.Succeed)
            {
                task.SubTasks.Add(_mapper.Map<TaskItemViewModel>(response.Result));
                await NavigationService.Close(this, true);
            }
            else
            {
                Logger.Error($"An unknown error occurred while trying to save the subtask. Error = {response.Message}");
                _dialogService.ShowErrorToast(TextProvider.Get("DatabaseUnknownError"));
            }
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