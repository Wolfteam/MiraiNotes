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
        private readonly IDialogService _dialogService;
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
                ItemId = SyncBgTaskIntervals.EACH_3_HOURS.ToString(),
                Text = GetText("Sync3")
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_6_HOURS.ToString(),
                Text = GetText("Sync6")
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_12_HOURS.ToString(),
                Text = GetText("Sync12")
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_24_HOURS.ToString(),
                Text = GetText("Sync24")
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
                AppSettings.SyncBackgroundTaskInterval = selectedInterval;
                //TODO: REGISTER BG TASK ?
                //_backgroundTaskManagerService.RegisterBackgroundTasks(BackgroundTaskType.SYNC, true);
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
            IDialogService dialogService)
            : base(textProvider, messenger, logger.ForContext<SettingsMainViewModel>(), navigationService, appSettings)
        {
            _dialogService = dialogService;

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