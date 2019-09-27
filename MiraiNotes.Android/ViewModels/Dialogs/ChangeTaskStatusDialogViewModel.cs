using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class ChangeTaskStatusDialogViewModel : BaseConfirmationDialogViewModel<TaskItemViewModel, bool>
    {
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;

        public ChangeTaskStatusDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            IMiraiNotesDataService dataService,
            IDialogService dialogService)
            : base(textProvider, messenger, logger.ForContext<ChangeTaskStatusDialogViewModel>(), navigationService, appSettings)
        {
            _dataService = dataService;
            _dialogService = dialogService;
        }

        public override void Prepare(TaskItemViewModel parameter)
        {
            base.Prepare(parameter);

            string statusMessage =
                $"{(Parameter.IsCompleted ? GetText("Incompleted") : GetText("Completed"))}";


            Title = GetText("Confirmation");
            ContentText = GetText("MarkTaskAsConfirmation", Parameter.Title, statusMessage);
            OkText = GetText("Yes");
            CancelText = GetText("No");
        }

        public override void RegisterMessages()
        {
        }

        public override void SetCommands()
        {
            OkCommand = new MvxAsyncCommand(async () =>
            {
                var newStatus = Parameter.IsCompleted
                    ? GoogleTaskStatus.NEEDS_ACTION
                    : GoogleTaskStatus.COMPLETED;
                await ChangeTaskStatus(Parameter, newStatus);
            });
            CloseCommand = new MvxAsyncCommand(() => NavigationService.Close(this));
        }

        private async Task ChangeTaskStatus(TaskItemViewModel task, GoogleTaskStatus newStatus)
        {
            Messenger.Publish(new ShowProgressOverlayMsg(this));

            string statusMessage =
                $"{(newStatus == GoogleTaskStatus.COMPLETED ? GetText("Completed") : GetText("Incompleted"))}";

            var response = await _dataService
                .TaskService
                .ChangeTaskStatusAsync(task.TaskID, newStatus);

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            if (!response.Succeed)
            {
                Logger.Error(
                    $"{nameof(ChangeTaskStatus)}: An error occurred while trying to mark {task.Title} as {statusMessage}." +
                    $"Error = {response.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                await NavigationService.Close(this, false);
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

            _dialogService.ShowSnackBar(GetText("TaskStatusChanged", task.Title, statusMessage));
            await NavigationService.Close(this, true);
        }
    }
}