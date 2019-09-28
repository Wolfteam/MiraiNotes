using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
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

        public DeleteTaskDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            IMiraiNotesDataService dataService,
            IDialogService dialogService)
            : base(textProvider, messenger, logger.ForContext<DeleteTaskDialogViewModel>(), navigationService, appSettings)
        {
            _dataService = dataService;
            _dialogService = dialogService;
        }

        public override void Prepare(TaskItemViewModel parameter)
        {
            base.Prepare(parameter);
            Title = GetText("Confirmation");
            ContentText = GetText("DeleteTaskConfirmation");
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
            Messenger.Publish(new ShowProgressOverlayMsg(this));

            var deleteResponse = await _dataService
                .TaskService
                .RemoveTaskAsync(Parameter.TaskID);

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

            Messenger.Publish(new TaskDeletedMsg(this, Parameter.TaskID, Parameter.ParentTask));
            await NavigationService.Close(this, true);
        }
    }
}