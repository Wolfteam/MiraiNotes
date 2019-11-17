using MiraiNotes.Core.Enums;

namespace MiraiNotes.Abstractions.Services
{
    public interface IAppSettingsService
    {
        AppThemeType AppTheme { get; set; }

        string AppHexAccentColor { get; set; }

        SyncBgTaskIntervals SyncBackgroundTaskInterval { get; set; }

        bool RunSyncBackgroundTaskAfterStart { get; set; }

        bool ShowToastNotificationAfterFullSync { get; set; }

        bool ShowToastNotificationForCompletedTasks { get; set; }

        TaskSortType DefaultTaskSortOrder { get; set; }

        bool AskForPasswordWhenAppStarts { get; set; }

        bool AskForFingerPrintWhenAppStarts { get; set; }

        bool ShowCompletedTasks { get; set; }

        TaskListSortType DefaultTaskListSortOrder { get; set; }

        void ResetAppSettings();

        bool RunFullSyncAfterSwitchingAccounts { get; set; }

        AppLanguageType AppLanguage { get; set; }

        string SelectedTaskListId { get; set; }

        bool AskBeforeDiscardChanges { get; set; }

        string CurrentAppMigration { get; set; }
    }
}
