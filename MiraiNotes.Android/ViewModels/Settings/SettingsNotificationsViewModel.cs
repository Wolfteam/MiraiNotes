using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Plugin.Messenger;

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
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IAppSettingsService appSettings,
            IDialogService dialogService)
            : base(textProvider, messenger, appSettings)
        {
            _dialogService = dialogService;

            SetCommands();
        }

        private void SetCommands()
        {
            ShowToastNotificationAfterFullSyncCommand = new MvxCommand(
                () => ShowToastNotificationAfterFullSync = !ShowToastNotificationAfterFullSync);
        }
    }
}