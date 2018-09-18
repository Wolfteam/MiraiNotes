using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MiraiNotes.UWP.Delegates;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MiraiNotes.UWP.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        #region Members
        private readonly IApplicationSettingsService _appSettings;
        private readonly ICustomDialogService _dialogService;
        private readonly IDispatcherHelper _dispatcher;
        private readonly IMessenger _messenger;
        private readonly IBackgroundTaskManagerService _backgroundTaskManagerService;

        private bool _isBackButtonVisible;
        private string _currentPageText;
        #endregion

        #region Events
        public event SettingsNavigationRequest NavigationRequest;
        #endregion


        #region Properties
        public string CurrentPageText
        {
            get => _currentPageText;
            set => Set(ref _currentPageText, value);
        }

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

        public List<ItemModel> TaskListSortTypes { get; } = new List<ItemModel>
        {
            new ItemModel
            {
                ItemID = TaskListSortType.BY_NAME_ASC.ToString(),
                Text = "By name asc"
            },
            new ItemModel
            {
                ItemID = TaskListSortType.BY_NAME_DESC.ToString(),
                Text = "By name desc"
            },
            new ItemModel
            {
                ItemID = TaskListSortType.BY_UPDATED_DATE_ASC.ToString(),
                Text = "By updated date asc"
            },
            new ItemModel
            {
                ItemID = TaskListSortType.BY_UPDATED_DATE_DESC.ToString(),
                Text = "By updated date desc"
            }
        };

        public List<ItemModel> TasksSortTypes { get; } = new List<ItemModel>
        {
            new ItemModel
            {
                ItemID = TaskSortType.BY_NAME_ASC.ToString(),
                Text = "By name asc"
            },
            new ItemModel
            {
                ItemID = TaskSortType.BY_NAME_DESC.ToString(),
                Text = "By name desc"
            },
            new ItemModel
            {
                ItemID = TaskSortType.BY_UPDATED_DATE_ASC.ToString(),
                Text = "By updated date asc"
            },
            new ItemModel
            {
                ItemID = TaskSortType.BY_UPDATED_DATE_DESC.ToString(),
                Text = "By updated date desc"
            }
        };

        public List<ItemModel> SyncBgTaskIntervalTypes { get; } = new List<ItemModel>
        {
            new ItemModel
            {
                ItemID = SyncBgTaskIntervals.NEVER.ToString(),
                Text = "Never"
            },
            new ItemModel
            {
                ItemID = SyncBgTaskIntervals.EACH_3_HOURS.ToString(),
                Text = "Each 3 hours"
            },
            new ItemModel
            {
                ItemID = SyncBgTaskIntervals.EACH_6_HOURS.ToString(),
                Text = "Each 6 hours"
            },
            new ItemModel
            {
                ItemID = SyncBgTaskIntervals.EACH_12_HOURS.ToString(),
                Text = "Each 12 hours"
            },
            new ItemModel
            {
                ItemID = SyncBgTaskIntervals.EACH_24_HOURS.ToString(),
                Text = "Each 24 hours"
            }
        };
        #endregion


        #region General Settings Properties
        public bool AskForPasswordWhenAppStarts
        {
            get => _appSettings.AskForPasswordWhenAppStarts;
            set
            {
                if (value)
                {
                    _dispatcher.CheckBeginInvokeOnUi(async () =>
                    {
                        bool isPasswordSaved = await _dialogService.ShowCustomDialog(CustomDialogType.PASSWORD_DIALOG);
                        _appSettings.AskForPasswordWhenAppStarts = isPasswordSaved;
                        RaisePropertyChanged(nameof(AskForPasswordWhenAppStarts));
                    });
                }
                else
                    _appSettings.AskForPasswordWhenAppStarts = value;
            }
        }

        public ItemModel CurrentTaskListSortOrder
        {
            get
            {
                var currentSelectedSortType = _appSettings.DefaultTaskListSortOrder;
                return TaskListSortTypes.FirstOrDefault(i => i.ItemID == currentSelectedSortType.ToString());
            }
            set
            {
                var selectedSortType = (TaskListSortType)Enum.Parse(typeof(TaskListSortType), value.ItemID);
                _appSettings.DefaultTaskListSortOrder = selectedSortType;
                _messenger.Send(selectedSortType, $"{MessageType.DEFAULT_TASK_LIST_SORT_ORDER_CHANGED}");
            }
        }

        public ItemModel CurrentTaskSortOrder
        {
            get
            {
                var currentSelectedSortType = _appSettings.DefaultTaskSortOrder;
                return TasksSortTypes.FirstOrDefault(i => i.ItemID == currentSelectedSortType.ToString());
            }
            set
            {
                var selectedSortType = (TaskSortType)Enum.Parse(typeof(TaskSortType), value.ItemID);
                _appSettings.DefaultTaskSortOrder = selectedSortType;
                _messenger.Send(selectedSortType, $"{MessageType.DEFAULT_TASK_SORT_ORDER_CHANGED}");
            }
        }

        public bool ShowCompletedTasks
        {
            get => _appSettings.ShowCompletedTasks;
            set => _appSettings.ShowCompletedTasks = value;
        }
        #endregion


        #region Synchronization Settings Properties
        public bool RunSyncBackgroundTaskAfterStart
        {
            get => _appSettings.RunSyncBackgroundTaskAfterStart;
            set => _appSettings.RunSyncBackgroundTaskAfterStart = value;
        }

        public ItemModel CurrentSyncBackgroundTaskInterval
        {
            get
            {
                var currentInterval = _appSettings.SyncBackgroundTaskInterval;
                return SyncBgTaskIntervalTypes.FirstOrDefault(s => s.ItemID == currentInterval.ToString());
            }
            set
            {
                var selectedInterval = (SyncBgTaskIntervals)Enum.Parse(typeof(SyncBgTaskIntervals), value.ItemID);
                _appSettings.SyncBackgroundTaskInterval = selectedInterval;
                _backgroundTaskManagerService.RegisterBackgroundTasks(BackgroundTaskType.SYNC, true);
            }
        }
        #endregion


        #region Notification Settings Properties
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


        #region Commands
        public ICommand NavigationRequestCommand { get; set; }
        #endregion

        public SettingsPageViewModel(
            IApplicationSettingsService appSettings,
            ICustomDialogService dialogService,
            IDispatcherHelper dispatcher,
            IMessenger messenger,
            IBackgroundTaskManagerService backgroundTaskManagerService)
        {
            _appSettings = appSettings;
            _dialogService = dialogService;
            _dispatcher = dispatcher;
            _messenger = messenger;
            _backgroundTaskManagerService = backgroundTaskManagerService;

            NavigationRequestCommand = new RelayCommand<SettingsPageType>
                ((page) => NavigationRequest?.Invoke(page));
        }
    }
}
