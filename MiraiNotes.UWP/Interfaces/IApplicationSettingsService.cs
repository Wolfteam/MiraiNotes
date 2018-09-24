using MiraiNotes.UWP.Models;

namespace MiraiNotes.UWP.Interfaces
{
    public interface IApplicationSettingsService
    {
        AppThemeType AppTheme { get; set; }

        SyncBgTaskIntervals SyncBackgroundTaskInterval { get; set; }

        bool RunSyncBackgroundTaskAfterStart { get; set; }

        bool ShowToastNotificationAfterFullSync { get; set; }

        bool ShowToastNotificationForCompletedTasks { get; set; }

        TaskSortType DefaultTaskSortOrder { get; set; }

        bool AskForPasswordWhenAppStarts { get; set; }

        bool ShowCompletedTasks { get; set; }

        TaskListSortType DefaultTaskListSortOrder { get; set; }
    }
}
