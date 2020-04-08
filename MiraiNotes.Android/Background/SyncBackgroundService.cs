using Android.App;
using Android.Content;
using Android.Runtime;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Services;
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

        private IAndroidAppSettings _appSettings;

        public SyncBackgroundService()
            : base(nameof(SyncBackgroundService))
        {

        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);
            var textProvider = Mvx.IoCProvider.Resolve<ITextProvider>();
            var notificationService = Mvx.IoCProvider.Resolve<INotificationService>() as NotificationService;
            _appSettings = Mvx.IoCProvider.Resolve<IAppSettingsService>() as IAndroidAppSettings;

            var notification = notificationService
                .BuildSimpleNotification(textProvider.Get("Syncing"), textProvider.Get("SyncRunning"))
                .SetOngoing(true)
                .Build();

            StartForeground(SyncServiceId, notification);

            // This tells Android not to restart the service if it is killed to reclaim resources.
            return StartCommandResult.Sticky;
        }


        protected override void OnHandleIntent(Intent intent)
        {
            base.OnHandleIntent(intent);
            try
            {
                _appSettings.SetBoolean(IsServiceRunningKey, true);
                int taskListId = intent?.Extras?.GetInt(TaskListIdToSyncKey, 0) ?? 0;

                var synTask = new SyncTask(true, taskListId > 0 ? taskListId : (int?)null);
                synTask.Sync().GetAwaiter().GetResult();
            }
            catch (Exception)
            {
            }
            finally
            {
                _appSettings.SetBoolean(IsServiceRunningKey, false);
                StopSelf();
            }
        }
    }
}