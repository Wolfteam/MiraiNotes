using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Android.Models.Results;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class ChangeTaskStatusDialogViewModel
        : BaseConfirmationDialogViewModel<ChangeTaskStatusDialogViewModelParameter, ChangeTaskStatusDialogViewModelResult>
    {
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;

        public ChangeTaskStatusDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService,
            IMiraiNotesDataService dataService,
            IDialogService dialogService)
            : base(textProvider, messenger, logger.ForContext<ChangeTaskStatusDialogViewModel>(), navigationService, appSettings, telemetryService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
        }

        public override void Prepare(ChangeTaskStatusDialogViewModelParameter parameter)
        {
            base.Prepare(parameter);

            Title = GetText("Confirmation");
            string statusMessage = !parameter.IsMultipleTasks
                ? $"{(Parameter.Task.IsCompleted ? GetText("Incompleted") : GetText("Completed"))}"
                : $"{GetText("Completed")}";
            if (!parameter.IsMultipleTasks)
                ContentText = GetText("MarkTaskAsConfirmation", Parameter.Task.Title, statusMessage);
            else
                ContentText = GetText("MarkMultipleTasksAsCompletedConfirmation", $"{parameter.Tasks.Count}");
            OkText = GetText("Yes");
            CancelText = GetText("No");
        }

        public override void SetCommands()
        {
            base.SetCommands();

            OkCommand = new MvxAsyncCommand
                (async () => await ChangeTaskStatus());

            CloseCommand = new MvxAsyncCommand(
                async () => await NavigationService.Close(this));
        }

        private async Task<bool> ChangeTaskStatus(TaskItemViewModel task, GoogleTaskStatus newStatus)
        {
            Messenger.Publish(new ShowProgressOverlayMsg(this));

            var response = await _dataService
                .TaskService
                .ChangeTaskStatusAsync(task.GoogleId, newStatus);

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            if (!response.Succeed)
            {
                Logger.Error(
                    $"{nameof(ChangeTaskStatus)}: An error occurred while trying to mark {task.Title} as {GetStatusMsgToUse(newStatus)}." +
                    $"Error = {response.Message}");
                return false;
            }

            task.Status = response.Result.Status;
            task.CompletedOn = response.Result.CompletedOn;
            task.UpdatedAt = response.Result.UpdatedAt;

            Messenger.Publish(new TaskStatusChangedMsg(
                this,
                task.GoogleId,
                task.ParentTask,
                task.CompletedOn,
                task.UpdatedAt,
                task.Status));
            return true;
        }

        private async Task ChangeTaskStatus()
        {
            var results = new List<bool>();
            var newStatus = Parameter.IsMultipleTasks
                ? GoogleTaskStatus.COMPLETED
                : Parameter.Task.IsCompleted
                    ? GoogleTaskStatus.NEEDS_ACTION
                    : GoogleTaskStatus.COMPLETED;
            if (Parameter.IsMultipleTasks)
            {
                foreach (var task in Parameter.Tasks)
                {
                    bool result = await ChangeTaskStatus(task, newStatus);
                    results.Add(result);
                }
            }
            else
            {
                bool result = await ChangeTaskStatus(Parameter.Task, newStatus);
                results.Add(result);
            }

            if (results.Any(r => !r))
            {
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                var result = Parameter.IsMultipleTasks
                    ? ChangeTaskStatusDialogViewModelResult.Partial()
                    : ChangeTaskStatusDialogViewModelResult.Changed(false);
                await NavigationService.Close(this, result);
            }
            else
            {
                if (!Parameter.IsMultipleTasks)
                    _dialogService.ShowSnackBar(GetText("TaskStatusChanged", Parameter.Task.Title, GetStatusMsgToUse(newStatus)));
                else
                    _dialogService.ShowSnackBar(GetText("SelectedTasksWereMarkedAsCompleted"));
                var result = ChangeTaskStatusDialogViewModelResult.Changed(true);
                await NavigationService.Close(this, result);
            }
        }

        private string GetStatusMsgToUse(GoogleTaskStatus newStatus)
            => $"{(newStatus == GoogleTaskStatus.COMPLETED ? GetText("Completed") : GetText("Incompleted"))}";
    }
}