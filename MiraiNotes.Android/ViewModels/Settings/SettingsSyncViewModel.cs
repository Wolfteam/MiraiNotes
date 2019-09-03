using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MvvmCross.Localization;
using MvvmCross.Plugin.Messenger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsSyncViewModel : BaseViewModel
    {
        #region Members
        private readonly IAppSettingsService _appSettings;
        private readonly IDialogService _dialogService;
        #endregion


        #region Properties
        public List<ItemModel> SyncBgTaskIntervalTypes { get; } = new List<ItemModel>
        {
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.NEVER.ToString(),
                Text = "Never"
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_3_HOURS.ToString(),
                Text = "Each 3 hours"
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_6_HOURS.ToString(),
                Text = "Each 6 hours"
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_12_HOURS.ToString(),
                Text = "Each 12 hours"
            },
            new ItemModel
            {
                ItemId = SyncBgTaskIntervals.EACH_24_HOURS.ToString(),
                Text = "Each 24 hours"
            }
        };



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
                return SyncBgTaskIntervalTypes.FirstOrDefault(s => s.ItemId == currentInterval.ToString());
            }
            set
            {
                var selectedInterval = (SyncBgTaskIntervals)Enum.Parse(typeof(SyncBgTaskIntervals), value.ItemId, true);
                _appSettings.SyncBackgroundTaskInterval = selectedInterval;
                //_backgroundTaskManagerService.RegisterBackgroundTasks(BackgroundTaskType.SYNC, true);
            }
        }

        public bool RunFullSyncAfterSwitchingAccounts
        {
            get => _appSettings.RunFullSyncAfterSwitchingAccounts;
            set => _appSettings.RunFullSyncAfterSwitchingAccounts = value;
        }
        #endregion


        public SettingsSyncViewModel(
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