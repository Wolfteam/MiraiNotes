using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsNotificationsViewModel : BaseViewModel
    {
        #region Members
        private readonly IDialogService _dialogService;
        #endregion

        #region Properties
        public bool ShowToastNotificationAfterFullSync
        {
            get => AppSettings.ShowToastNotificationAfterFullSync;
            set
            {
                AppSettings.ShowToastNotificationAfterFullSync = value;
                RaisePropertyChanged(() => ShowToastNotificationAfterFullSync);
            }
        }

        public bool ShowToastNotificationForCompletedTasks
        {
            get => AppSettings.ShowToastNotificationForCompletedTasks;
            set => AppSettings.ShowToastNotificationForCompletedTasks = value;
        }
        #endregion

        #region Commands
        public IMvxCommand ShowToastNotificationAfterFullSyncCommand { get; private set; }
        #endregion


        public SettingsNotificationsViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            IDialogService dialogService,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<SettingsMainViewModel>(), navigationService, appSettings, telemetryService)
        {
            _dialogService = dialogService;
        }

        public override void SetCommands()
        {
            base.SetCommands();
            ShowToastNotificationAfterFullSyncCommand = new MvxCommand(
                () => ShowToastNotificationAfterFullSync = !ShowToastNotificationAfterFullSync);
        }
    }
}