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

        public SyncBgTaskIntervals SyncBackgroundTaskInterval
        {
            get => (SyncBgTaskIntervals)(_settings[nameof(SyncBackgroundTaskInterval)] ?? SyncBgTaskIntervals.NEVER);
            set => _settings[nameof(SyncBackgroundTaskInterval)] = value;
        }

        public bool SyncBackgroundTaskAfterStart
        {
            get => (bool)(_settings[nameof(SyncBackgroundTaskAfterStart)] ?? false);
            set => _settings[nameof(SyncBackgroundTaskAfterStart)] = value;
        }


        public bool ShowToastNotificationAfterFullSync
        {
            get => (bool)(_settings[nameof(ShowToastNotificationAfterFullSync)] ?? false);
            set => _settings[nameof(ShowToastNotificationAfterFullSync)] = value;
        }

        //public static bool ShowToastNotificationForTasks
        //{
        //    get => (bool)(_settings[nameof(ShowToastNotificationAfterFullSync)] ?? false);
        //    set => _settings[nameof(ShowToastNotificationAfterFullSync)] = value;
        //}

        //public static TaskSortType DefaultTaskListSortOrder
        //{
        //    get => (TaskSortType)(_settings[nameof(DefaultTaskListSortOrder)] ?? TaskSortType.BY_NAME_ASC);
        //    set => _settings[nameof(DefaultTaskListSortOrder)] = value;
        //}

        public TaskSortType DefaultTaskSortOrder
        {
            get => (TaskSortType)(_settings[nameof(DefaultTaskSortOrder)] ?? TaskSortType.BY_NAME_ASC);
            set => _settings[nameof(DefaultTaskSortOrder)] = value;
        }

        public bool AskForPasswordWhenAppStarts
        {
            get => (bool)(_settings[nameof(AskForPasswordWhenAppStarts)] ?? false);
            set => _settings[nameof(AskForPasswordWhenAppStarts)] = value;
        }
    }
}
