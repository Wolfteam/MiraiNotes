using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MvvmCross.Localization;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsNotificationsViewModel : BaseViewModel
    {
        #region Members
        private readonly IAppSettingsService _appSettings;
        private readonly IDialogService _dialogService;
        #endregion


        #region Properties
        public bool ShowToastNotificationAfterFullSync
        {
            get => _appSettings.ShowToastNotificationAfterFullSync;
            set => _appSettings.ShowToastNotificationAfterFullSync = value;
        }

        public bool ShowToastNotificationForCompletedTasks
        {
            get => _appSettings.ShowToastNotificationForCompletedTasks;
            set => _appSettings.ShowToastNotificationForCompletedTasks = value;
        }
        #endregion

        public SettingsNotificationsViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IAppSettingsService appSettings,
            IDialogService dialogService)
            : base(textProvider, messenger)
        {
            _appSettings = appSettings;
            _dialogService = dialogService;
        }
    }
}