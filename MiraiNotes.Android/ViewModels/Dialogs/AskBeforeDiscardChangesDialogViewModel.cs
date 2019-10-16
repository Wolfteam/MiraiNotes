using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class AskBeforeDiscardChangesDialogViewModel : BaseConfirmationDialogViewModel<TaskItemViewModel, bool>
    {
        public AskBeforeDiscardChangesDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<AskBeforeDiscardChangesDialogViewModel>(), navigationService, appSettings, telemetryService)
        {
        }

        public override void Prepare(TaskItemViewModel parameter)
        {
            base.Prepare(parameter);

            ContentText = TextProvider.Get("WannaDiscardChanges");
        }

        public override void SetCommands()
        {
            base.SetCommands();
            OkCommand = new MvxAsyncCommand(() => NavigationService.Close(this, true));
            CloseCommand = new MvxAsyncCommand(() => NavigationService.Close(this, false));
        }
    }
}