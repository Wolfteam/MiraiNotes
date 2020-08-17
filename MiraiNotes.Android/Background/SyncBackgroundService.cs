using Android.App;
using Android.Content;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MvvmCross;
using MvvmCross.Platforms.Android.Services;
using System;

namespace MiraiNotes.Android.Background
{
    [Service]
    public class SyncBackgroundService : MvxIntentService
    {
        public const int SyncServiceId = 101;
        public const string IsServiceRunningKey = "IsSyncServiceRunning";
        public const string TaskListIdToSyncKey = "TaskListIdToSync";
        public const string StartedManuallyKey = "StartedManually";

        public SyncBackgroundService()
            : base(nameof(SyncBackgroundService))
        {
        }

        protected override void OnHandleIntent(Intent intent)
        {
            base.OnHandleIntent(intent);
            var appSettings = Mvx.IoCProvider.Resolve<IAppSettingsService>() as IAndroidAppSettings;
            try
            {
                var textProvider = Mvx.IoCProvider.Resolve<ITextProvider>();
                var notificationService = Mvx.IoCProvider.Resolve<IAndroidNotificationService>();

                var notification = notificationService
                    .BuildSimpleNotification(textProvider.Get("Syncing"), textProvider.Get("SyncRunning"), true)
                    .SetOngoing(true)
                    .Build();

                StartForeground(SyncServiceId, notification);
                appSettings.SetBoolean(IsServiceRunningKey, true);
                int taskListId = intent?.Extras?.GetInt(TaskListIdToSyncKey, 0) ?? 0;
                bool startedManually = intent?.Extras?.GetBoolean(StartedManuallyKey, false) ?? false;

                var synTask = new SyncTask(startedManually, taskListId > 0 ? taskListId : (int?)null);
                synTask.Sync().GetAwaiter().GetResult();
            }
            catch (Exception)
            {
            }
            finally
            {
                appSettings.SetBoolean(IsServiceRunningKey, false);
                StopSelf();
            }
        }
    }
}