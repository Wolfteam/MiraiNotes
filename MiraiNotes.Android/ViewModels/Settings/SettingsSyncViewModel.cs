using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsSyncViewModel : BaseViewModel
    {
        #region Members
        private readonly IBackgroundTaskManagerService _backgroundTaskManager;
        #endregion

        #region Properties
        public List<ItemModel> SyncBgTaskIntervalTypes => new List<ItemModel>
        {
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.NEVER.ToString(),
                Text = GetText("Never")
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_30_MIN.ToString(),
                Text = GetText("Sync30m")
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_1_HOUR.ToString(),
                Text = GetText("Sync1h")
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_3_HOURS.ToString(),
                Text = GetText("Sync3h")
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_6_HOURS.ToString(),
                Text = GetText("Sync6h")
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_12_HOURS.ToString(),
                Text = GetText("Sync12h")
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_24_HOURS.ToString(),
                Text = GetText("Sync24h")
            }
        };

        public bool RunSyncBackgroundTaskAfterStart
        {
            get => AppSettings.RunSyncBackgroundTaskAfterStart;
            set
            {
                AppSettings.RunSyncBackgroundTaskAfterStart = value;
                RaisePropertyChanged(() => RunSyncBackgroundTaskAfterStart);
            }
        }

        public ItemModel CurrentSyncBackgroundTaskInterval
        {
            get
            {
                var currentInterval = AppSettings.SyncBackgroundTaskInterval;
                return SyncBgTaskIntervalTypes.FirstOrDefault(s => s.ItemId == currentInterval.ToString());
            }
            set
            {
                var selectedInterval = (SyncBgTaskIntervals)Enum.Parse(typeof(SyncBgTaskIntervals), value.ItemId, true);
                if (AppSettings.SyncBackgroundTaskInterval == selectedInterval)
                    return;

                AppSettings.SyncBackgroundTaskInterval = selectedInterval;
                if (selectedInterval == SyncBgTaskIntervals.NEVER)
                    _backgroundTaskManager.UnregisterBackgroundTasks(BackgroundTaskType.SYNC);
                else
                    _backgroundTaskManager.RegisterBackgroundTasks(BackgroundTaskType.SYNC);
            }
        }

        public bool RunFullSyncAfterSwitchingAccounts
        {
            get => AppSettings.RunFullSyncAfterSwitchingAccounts;
            set
            {
                AppSettings.RunFullSyncAfterSwitchingAccounts = value;
                RaisePropertyChanged(() => RunFullSyncAfterSwitchingAccounts);
            }
        }
        #endregion


        #region Commands
        public IMvxCommand RunSyncBackgroundTaskAfterStartCommand { get; private set; }
        public IMvxCommand RunFullSyncAfterSwitchingAccountsCommand { get; private set; }
        #endregion

        public SettingsSyncViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            IBackgroundTaskManagerService backgroundTaskManager)
            : base(textProvider, messenger, logger.ForContext<SettingsMainViewModel>(), navigationService, appSettings)
        {
            _backgroundTaskManager = backgroundTaskManager;
            SetCommands();
        }

        private void SetCommands()
        {
            RunSyncBackgroundTaskAfterStartCommand = new MvxCommand(
                () => RunSyncBackgroundTaskAfterStart = !RunSyncBackgroundTaskAfterStart);

            RunFullSyncAfterSwitchingAccountsCommand = new MvxCommand(
                () => RunFullSyncAfterSwitchingAccounts = !RunFullSyncAfterSwitchingAccounts);
        }
    }
}