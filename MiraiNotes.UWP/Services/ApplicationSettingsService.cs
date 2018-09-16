using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;

namespace MiraiNotes.UWP.Services
{
    public class ApplicationSettingsService : IApplicationSettingsService
    {
        private readonly IApplicationSettingsServiceBase _settings;

        public ApplicationSettingsService(IApplicationSettingsServiceBase settings)
        {
            _settings = settings;
        }

        #region General
        //public int AppTheme
        //{
        //    get => (TaskSortType)(_settings[nameof(AppTheme)] ?? TaskSortType.BY_NAME_ASC);
        //    set => _settings[nameof(AppTheme)] = (int)value;
        //}

        //public static TaskSortType DefaultTaskListSortOrder
        //{
        //    get => (TaskSortType)(_settings[nameof(DefaultTaskListSortOrder)] ?? TaskSortType.BY_NAME_ASC);
        //    set => _settings[nameof(DefaultTaskListSortOrder)] = value;
        //}
        public TaskSortType DefaultTaskSortOrder
        {
            get => (TaskSortType)(_settings[nameof(DefaultTaskSortOrder)] ?? TaskSortType.BY_NAME_ASC);
            set => _settings[nameof(DefaultTaskSortOrder)] = (int)value;
        }

        public bool ShowCompletedTasks
        {
            get => (bool)(_settings[nameof(ShowCompletedTasks)] ?? false);
            set => _settings[nameof(ShowCompletedTasks)] = value;
        }

        public bool AskForPasswordWhenAppStarts
        {
            get => (bool)(_settings[nameof(AskForPasswordWhenAppStarts)] ?? false);
            set => _settings[nameof(AskForPasswordWhenAppStarts)] = value;
        }
        #endregion


        #region Synchronization
        public SyncBgTaskIntervals SyncBackgroundTaskInterval
        {
            get => (SyncBgTaskIntervals)(_settings[nameof(SyncBackgroundTaskInterval)] ?? SyncBgTaskIntervals.NEVER);
            set => _settings[nameof(SyncBackgroundTaskInterval)] = (int)value;
        }

        public bool RunSyncBackgroundTaskAfterStart
        {
            get => (bool)(_settings[nameof(RunSyncBackgroundTaskAfterStart)] ?? false);
            set => _settings[nameof(RunSyncBackgroundTaskAfterStart)] = value;
        }
        #endregion


        #region Notifications
        public bool ShowToastNotificationAfterFullSync
        {
            get => (bool)(_settings[nameof(ShowToastNotificationAfterFullSync)] ?? false);
            set => _settings[nameof(ShowToastNotificationAfterFullSync)] = value;
        }

        public bool ShowToastNotificationForCompletedTasks
        {
            get => (bool)(_settings[nameof(ShowToastNotificationForCompletedTasks)] ?? false);
            set => _settings[nameof(ShowToastNotificationForCompletedTasks)] = value;
        }
        #endregion
    }
}
