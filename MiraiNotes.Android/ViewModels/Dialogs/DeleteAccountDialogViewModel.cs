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
    public class DeleteAccountDialogViewModel : BaseConfirmationDialogViewModel<GoogleUserViewModel, bool>
    {
        public DeleteAccountDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<DeleteAccountDialogViewModel>(), navigationService, appSettings, telemetryService)
        {
        }

        public override void Prepare(GoogleUserViewModel parameter)
        {
            base.Prepare(parameter);

            Title = $"{GetText("Delete")} {Parameter.Fullname}";
            ContentText = GetText("DeleteThisAccountConfirmation");
            OkText = GetText("Yes");
            CancelText = GetText("No");
        }

        public override void SetCommands()
        {
            base.SetCommands();
            OkCommand = new MvxAsyncCommand(() => DeleteAccount());
            CloseCommand = new MvxAsyncCommand(() => NavigationService.Close(this, false));
        }


        private async Task DeleteAccount()
        {
            Messenger.Publish(new AccountChangeRequestMsg(this, true, false, Parameter));
            await NavigationService.Close(this, true);
        }
    }
}