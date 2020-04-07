using Android.App;
using Android.Content;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Services;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MvvmCross;
using MvvmCross.Platforms.Android.Services;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MiraiNotes.Android.Background
{
    [BroadcastReceiver(Enabled = true, Label = "Notification Broadcast Receiver")]
    public class NotificationSchedulerReceiver : MvxBroadcastReceiver
    {
        public const string LocalNotificationKey = "LocalNotification";
        public const string TaskReminderNotificationKey = "TaskReminderNotification";

        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);
            string extraA = intent.GetStringExtra(LocalNotificationKey);
            string extraB = intent.GetStringExtra(TaskReminderNotificationKey);

            if (string.IsNullOrEmpty(extraB))
            {
                var notif = JsonConvert.DeserializeObject<TaskNotification>(extraA);
                using var thread = new NotificationSchedulerTask(notif);
                thread.Run();
            }
            else
            {
                var notif = JsonConvert.DeserializeObject<TaskReminderNotification>(extraB);
                using var thread = new NotificationSchedulerTask(notif);
                thread.Run();
            }
        }

        //the setaction method is required, otherwise the extras gets loss  
        //https://stackoverflow.com/questions/3168484/pendingintent-works-correctly-for-the-first-notification-but-incorrectly-for-the
        public static Intent CreateIntent(int id, string key, string extra)
        {
            var intent = new Intent(Application.Context, typeof(NotificationSchedulerReceiver))
                .SetAction(nameof(NotificationSchedulerReceiver) + id);

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(extra))
            {
                intent.PutExtra(key, extra);
            }
            return intent;
        }

        public class NotificationSchedulerTask : Java.Lang.Thread
        {
            private readonly ILogger _logger;
            private readonly ITelemetryService _telemetryService;
            private readonly NotificationService _notificationService;
            private readonly IMiraiNotesDataService _dataService;
            private readonly TaskNotification _notification;
            private readonly TaskReminderNotification _reminderNotification;

            public bool IsAReminder
                => _reminderNotification != null;

            public NotificationSchedulerTask(TaskNotification notification) 
                : this()
            {
                _notification = notification;
            }

            public NotificationSchedulerTask(TaskReminderNotification notification) 
                : this()
            {
                _reminderNotification = notification;
            }

            private NotificationSchedulerTask()
            {
                _logger = Mvx.IoCProvider.Resolve<ILogger>().ForContext<NotificationSchedulerTask>();
                _telemetryService = Mvx.IoCProvider.Resolve<ITelemetryService>();
                _notificationService = Mvx.IoCProvider.Resolve<INotificationService>() as NotificationService;
                _dataService = Mvx.IoCProvider.Resolve<IMiraiNotesDataService>();
            }

            public override async void Run()
            {
                base.Run();

                await ShowNotification();
            }

            public async Task ShowNotification()
            {
                try
                {
                    bool isAppInForeground = AndroidUtils.IsAppInForeground();
                    if (!isAppInForeground)
                        _telemetryService.Init();

                    if (IsAReminder)
                    {
                        _logger.Information("Trying to show scheduled notification...");
                        _notificationService.ShowTaskReminderNotification(_reminderNotification);

                        _logger.Information(
                            $"Notification was shown, removing notification " +
                            $"date for taskId = {_reminderNotification.TaskId}");
                        await _dataService.TaskService.RemoveNotificationDate(
                            _reminderNotification.TaskId,
                            TaskNotificationDateType.REMINDER_DATE);
                    }
                    else
                    {
                        _logger.Information("Showing normal notification...");
                        _notificationService.ShowNotification(_notification);
                    }
                }
                catch (Exception e)
                {
                    _logger?.Error(e, $"An unknown error occurred while trying to show notification");
                    _telemetryService?.TrackError(e);
                }
            }
        }
    }
}