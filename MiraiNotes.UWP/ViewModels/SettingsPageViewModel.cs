using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MiraiNotes.UWP.Delegates;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System.Collections.Generic;
using System.Windows.Input;

namespace MiraiNotes.UWP.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        #region Members
        private readonly IApplicationSettingsService _appSettings;

        private bool _isBackButtonVisible;
        #endregion

        #region Events
        public SettingsNavigationRequest NavigationRequest;
        #endregion


        #region Properties
        public List<SettingsPageItem> SettingsPages { get; } = new List<SettingsPageItem>
        {
            new SettingsPageItem
            {
                Content = "Default sort order, themes, etc.",
                Header = "General",
                PageType = SettingsPageType.GENERAL
            },
            new SettingsPageItem
            {
                Content = "Background tasks intervals.",
                Header = "Synchronization",
                PageType = SettingsPageType.SYNCHRONIZATION
            },
            new SettingsPageItem
            {
                Content = "Toast notifications.",
                Header = "Notifications",
                PageType = SettingsPageType.NOTIFICATIONS
            },
            new SettingsPageItem
            {
                Content = "Github repo, contact information, donations.",
                Header = "About",
                PageType = SettingsPageType.ABOUT
            }
        };

        public bool IsBackButtonVisible
        {
            get => _isBackButtonVisible;
            set => Set(ref _isBackButtonVisible, value);
        }
        #endregion

        #region Settings Properties
        public bool AskForPasswordWhenAppStarts
        {
            get => _appSettings.AskForPasswordWhenAppStarts;
            set => _appSettings.AskForPasswordWhenAppStarts = value;
        }

        public TaskSortType DefaultTaskSortOrder
        {
            get => _appSettings.DefaultTaskSortOrder;
            set => _appSettings.DefaultTaskSortOrder = value;
        }

        public bool ShowToastNotificationAfterFullSync
        {
            get => _appSettings.ShowToastNotificationAfterFullSync;
            set => _appSettings.ShowToastNotificationAfterFullSync = value;
        }

        public bool SyncBackgroundTaskAfterStart
        {
            get => _appSettings.SyncBackgroundTaskAfterStart;
            set => _appSettings.SyncBackgroundTaskAfterStart = value;
        }

        public SyncBgTaskIntervals SyncBackgroundTaskInterval
        {
            get => _appSettings.SyncBackgroundTaskInterval;
            set => _appSettings.SyncBackgroundTaskInterval = value;
        }
        #endregion

        #region Commands
        public ICommand NavigationRequestCommand { get; set; }
        #endregion

        public SettingsPageViewModel(IApplicationSettingsService appSettings)
        {
            _appSettings = appSettings;

            NavigationRequestCommand = new RelayCommand<SettingsPageType>
                ((page) => NavigationRequest?.Invoke(page));
        }
    }
}
