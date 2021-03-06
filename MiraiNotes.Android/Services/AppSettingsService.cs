using Android.App;
using Android.Content;
using Android.Preferences;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MiraiNotes.Shared;
using System.Linq;

namespace MiraiNotes.Android.Services
{
    public class AppSettingsService : IAppSettingsService, IAndroidAppSettings
    {
        private ISharedPreferences GetSharedPreferences()
        {
            return PreferenceManager.GetDefaultSharedPreferences(Application.Context);
        }

        #region General

        public AppThemeType AppTheme
        {
            get
            {
                var currentTheme = (AppThemeType)GetInt(nameof(AppTheme));
                return currentTheme == AppThemeType.DEFAULT
                    ? AppThemeType.LIGHT
                    : currentTheme;
            }
            set => SetInt(nameof(AppTheme), (int)value);
        }

        public string AppHexAccentColor
        {
            get => GetString(nameof(AppHexAccentColor)) ?? AppConstants.AppAccentColors.First();
            set => SetString(nameof(AppHexAccentColor), value);
        }

        public TaskListSortType DefaultTaskListSortOrder
        {
            get => (TaskListSortType)GetInt(nameof(DefaultTaskListSortOrder));
            set => SetInt(nameof(DefaultTaskListSortOrder), (int)value);
        }

        public TaskSortType DefaultTaskSortOrder
        {
            get => (TaskSortType)GetInt(nameof(DefaultTaskSortOrder));
            set => SetInt(nameof(DefaultTaskSortOrder), (int)value);
        }

        public bool ShowCompletedTasks
        {
            get => GetBoolean(nameof(ShowCompletedTasks));
            set => SetBoolean(nameof(ShowCompletedTasks), value);
        }

        public bool AskForPasswordWhenAppStarts
        {
            get => GetBoolean(nameof(AskForPasswordWhenAppStarts));
            set => SetBoolean(nameof(AskForPasswordWhenAppStarts), value);
        }

        public bool AskForFingerPrintWhenAppStarts
        {
            get => GetBoolean(nameof(AskForFingerPrintWhenAppStarts));
            set => SetBoolean(nameof(AskForFingerPrintWhenAppStarts), value);
        }

        public AppLanguageType AppLanguage
        {
            get => (AppLanguageType)GetInt(nameof(AppLanguage));
            set => SetInt(nameof(AppLanguage), (int)value);
        }

        public int SelectedDbTaskListId
        {
            get => GetInt(nameof(SelectedDbTaskListId));
            set => SetInt(nameof(SelectedDbTaskListId), value);
        }

        public bool AskBeforeDiscardChanges
        {
            get => GetBoolean(nameof(AskBeforeDiscardChanges));
            set => SetBoolean(nameof(AskBeforeDiscardChanges), value);
        }

        public bool UseDarkAmoledTheme
        {
            get => GetBoolean(nameof(UseDarkAmoledTheme));
            set => SetBoolean(nameof(UseDarkAmoledTheme), value);
        }
        #endregion

        #region Synchronization

        public SyncBgTaskIntervals SyncBackgroundTaskInterval
        {
            get => (SyncBgTaskIntervals)GetInt(nameof(SyncBackgroundTaskInterval));
            set => SetInt(nameof(SyncBackgroundTaskInterval), (int)value);
        }

        public bool RunSyncBackgroundTaskAfterStart
        {
            get => GetBoolean(nameof(RunSyncBackgroundTaskAfterStart));
            set => SetBoolean(nameof(RunSyncBackgroundTaskAfterStart), value);
        }

        public bool RunFullSyncAfterSwitchingAccounts
        {
            get => GetBoolean(nameof(RunFullSyncAfterSwitchingAccounts));
            set => SetBoolean(nameof(RunFullSyncAfterSwitchingAccounts), value);
        }

        #endregion

        #region Notifications

        public bool ShowToastNotificationAfterFullSync
        {
            get => GetBoolean(nameof(ShowToastNotificationAfterFullSync));
            set => SetBoolean(nameof(ShowToastNotificationAfterFullSync), value);
        }

        public bool ShowToastNotificationForCompletedTasks
        {
            get => GetBoolean(nameof(ShowToastNotificationForCompletedTasks));
            set => SetBoolean(nameof(ShowToastNotificationForCompletedTasks), value);
        }
        public string CurrentAppMigration
        {
            get => GetString(nameof(CurrentAppMigration));
            set => SetString(nameof(CurrentAppMigration), value);
        }
        #endregion

        #region Helpers        
        public void ResetAppSettings()
        {
            AskForPasswordWhenAppStarts =
                RunSyncBackgroundTaskAfterStart =
                    ShowCompletedTasks =
                        ShowToastNotificationAfterFullSync =
                            ShowToastNotificationForCompletedTasks = false;
            SyncBackgroundTaskInterval = SyncBgTaskIntervals.NEVER;
            DefaultTaskListSortOrder = TaskListSortType.BY_NAME_ASC;
            DefaultTaskSortOrder = TaskSortType.BY_NAME_ASC;
        }

        public void SetString(string key, string value)
        {
            GetSharedPreferences().Edit().PutString(key, value).Apply();
        }

        public string GetString(string key)
        {
            return GetSharedPreferences().GetString(key, null);
        }

        public void SetInt(string key, int value)
        {
            GetSharedPreferences().Edit().PutInt(key, value).Apply();
        }

        public int GetInt(string key)
        {
            return GetSharedPreferences().GetInt(key, 0);
        }

        public void SetBoolean(string key, bool value)
        {
            GetSharedPreferences().Edit().PutBoolean(key, value).Apply();
        }

        public bool GetBoolean(string key)
        {
            return GetSharedPreferences().GetBoolean(key, false);
        }

        #endregion
    }
}