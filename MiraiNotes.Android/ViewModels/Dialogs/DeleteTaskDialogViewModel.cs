using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Shared.Helpers;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class DeleteTaskDialogViewModel : BaseConfirmationDialogViewModel<TaskItemViewModel, bool>
    {
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;

        public DeleteTaskDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService,
            IMiraiNotesDataService dataService,
            IDialogService dialogService,
            INotificationService notificationService)
            : base(textProvider, messenger, logger.ForContext<DeleteTaskDialogViewModel>(), navigationService, appSettings, telemetryService)
        {
            _dataService = dataService;
            _dialogService = dialogService;
            _notificationService = notificationService;
        }

        public override void Prepare(TaskItemViewModel parameter)
        {
            base.Prepare(parameter);
            Title = GetText("Confirmation");
            ContentText = GetText("DeleteTaskConfirmation", Parameter.Title);
            OkText = GetText("Yes");
            CancelText = GetText("No");
        }

        public override void SetCommands()
        {
            base.SetCommands();
            OkCommand = new MvxAsyncCommand(DeleteTask);
            CloseCommand = new MvxAsyncCommand(() => NavigationService.Close(this, false));
        }

        private async Task DeleteTask()
        {
            if (Parameter.HasParentTask && Parameter.IsNew || 
                Parameter.IsNew)
            {
                Messenger.Publish(new TaskDeletedMsg(this, Parameter.GoogleId, Parameter.ParentTask));
                await NavigationService.Close(this, true);
                return;
            }

            Messenger.Publish(new ShowProgressOverlayMsg(this));

            var deleteResponse = await _dataService
                .TaskService
                .RemoveTaskAsync(Parameter.GoogleId);

            if (TasksHelper.HasReminderId(Parameter.RemindOnGUID, out int id))
            {
                _notificationService.RemoveScheduledNotification(id);
            }

            if (Parameter.HasSubTasks)
            {
                foreach (var st in Parameter.SubTasks)
                    if (TasksHelper.HasReminderId(st.RemindOnGUID, out int stReminderId))
                        _notificationService.RemoveScheduledNotification(stReminderId);
            }

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            if (!deleteResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(DeleteTask)}: Couldn't delete the selected task." +
                    $"Error = {deleteResponse.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                await NavigationService.Close(this, false);
                return;
            }

            Messenger.Publish(new TaskDeletedMsg(this, Parameter.GoogleId, Parameter.ParentTask));
            await NavigationService.Close(this, true);
        }
    }
}