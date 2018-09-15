using MiraiNotes.UWP.Models;

namespace MiraiNotes.UWP.Interfaces
{
    public interface IApplicationSettingsService
    {
        SyncBgTaskIntervals SyncBackgroundTaskInterval { get; set; }

        bool SyncBackgroundTaskAfterStart { get; set; }

        bool ShowToastNotificationAfterFullSync { get; set; }

        TaskSortType DefaultTaskSortOrder { get; set; }

        bool AskForPasswordWhenAppStarts { get; set; }
    }
}
