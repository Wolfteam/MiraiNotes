﻿using AutoMapper;
using FluentValidation;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Extensions;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MiraiNotes.Shared.Helpers;
using MvvmCross;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            ITelemetryService telemetryService,
            IValidatorFactory validatorFactory,
            IMiraiNotesDataService dataService,
            IDialogService dialogService,
            IMapper mapper)
            : base(textProvider, messenger, logger.ForContext<AddSubTaskDialogViewModel>(), navigationService, appSettings, telemetryService)
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

            var task = Parameter.Task;
            var now = DateTimeOffset.UtcNow;
            if (task.IsNew)
            {
                var subTask = Mvx.IoCProvider.Resolve<TaskItemViewModel>();
                subTask.Title = SubTaskTitle.Trim();
                subTask.Status = GoogleTaskStatus.NEEDS_ACTION.GetString();
                subTask.CreatedAt = now;
                task.SubTasks.Add(subTask);

                _dialogService.ShowInfoToast(GetText("SubTaskWasAdded"));

                await NavigationService.Close(this, true);
            }
            else
            {
                Messenger.Publish(new ShowProgressOverlayMsg(this));

                var entity = new GoogleTask
                {
                    CreatedAt = now,
                    GoogleTaskID = Guid.NewGuid().ToString(),
                    LocalStatus = LocalStatus.CREATED,
                    ParentTask = task.GoogleId,
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

                    if (Parameter.Notify)
                        Messenger.Publish(new TaskSavedMsg(true, response.Result.GoogleTaskID));

                    _dialogService.ShowSucceedToast(GetText("SubTaskWasCreated"));

                    await NavigationService.Close(this, true);
                }
                else
                {
                    Logger.Error($"An unknown error occurred while trying to save the subtask. Error = {response.Message}");
                    _dialogService.ShowErrorToast(TextProvider.Get("DatabaseUnknownError"));
                }
            }

            if (!task.HasSubTasks && task.SubTasks.Any())
                task.HasSubTasks = true;
        }

        private void Validate()
        {
            Errors.Clear();
            var validationContext = new ValidationContext<AddSubTaskDialogViewModel>(this);
            var validationResult = _validator.Validate(validationContext);
            Errors.AddRange(validationResult.ToDictionary());
            RaisePropertyChanged(() => IsSaveButtonEnabled);
        }
    }
}