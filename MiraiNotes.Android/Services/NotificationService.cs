using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Background;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models;
using MiraiNotes.Android.Views.Activities;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using Newtonsoft.Json;
using System;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;

namespace MiraiNotes.Android.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ITextProvider _textProvider;

        private static string PackageName
            => Application.Context.PackageName;

        public static string GeneralChannelId
            => $"{PackageName}.general";

        private NotificationManager NotifManager
            => (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);

        public NotificationService(ITextProvider textProvider)
        {
            _textProvider = textProvider;
        }

        public void RemoveScheduledNotification(int id)
        {
            var intent = NotificationSchedulerReceiver.CreateIntent(id, null, null);
            var pendingIntent = PendingIntent.GetBroadcast(
                Application.Context,
                0,
                intent,
                PendingIntentFlags.CancelCurrent);

            var alarmManager = GetAlarmManager();
            alarmManager.Cancel(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(Application.Context);
            notificationManager.Cancel(id);
        }

        public void ScheduleNotification(TaskReminderNotification notification)
        {
            var now = DateTimeOffset.Now;
            bool showNow = notification.DeliveryOn <= now;
            if (showNow)
            {
                ShowTaskReminderNotification(notification);
                return;
            }

            var intent = NotificationSchedulerReceiver.CreateIntent(
                notification.Id,
                NotificationSchedulerReceiver.TaskReminderNotificationKey,
                JsonConvert.SerializeObject(notification));

            var pendingIntent = PendingIntent.GetBroadcast(
                Application.Context,
                0,
                intent,
                PendingIntentFlags.CancelCurrent);

            var future = (long)(notification.DeliveryOn - now).TotalMilliseconds;
            var milis = Java.Lang.JavaSystem.CurrentTimeMillis() + future;
            var alarmManager = GetAlarmManager();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                alarmManager.SetAlarmClock(new AlarmManager.AlarmClockInfo(milis, pendingIntent), pendingIntent);
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                alarmManager.SetExact(AlarmType.RtcWakeup, milis, pendingIntent);
            else
                alarmManager.Set(AlarmType.RtcWakeup, milis, pendingIntent);
        }

        public void ShowTaskReminderNotification(TaskReminderNotification notification)
        {
            //we create the pending intent when the user clicks the notification...
            var action = new NotificationAction
            {
                Action = NotificationActionType.OPEN_TASK,
                TaskId = notification.TaskId,
                TaskListId = notification.TaskListId
            };
            var clickIntent = MainActivity.CreateIntent(
                notification.Id,
                MainActivity.InitParamsKey,
                JsonConvert.SerializeObject(action));

            var pendingIntent = TaskStackBuilder.Create(Application.Context)
                .AddNextIntent(clickIntent)
                .GetPendingIntent(1, (int)PendingIntentFlags.UpdateCurrent);

            var localNotification = new TaskNotification
            {
                LargeContent =
                    $"{notification.TaskTitle}{System.Environment.NewLine}" +
                    $"{notification.TaskBody}",
                Title = notification.TaskListTitle,
                Id = notification.Id
            };
            var builder = BuildSimpleNotification(localNotification);
            builder.SetContentIntent(pendingIntent);

            //we create the pending intent when the user clicks the mark as completed button...
            var mcIntent = MarkTaskAsCompletedReceiver.CreateIntent(
                notification.Id,
                MarkTaskAsCompletedReceiver.MarkTaskAsCompletedKey,
                JsonConvert.SerializeObject(notification));

            var mcPendingIntent = PendingIntent.GetBroadcast(
                Application.Context,
                0,
                mcIntent,
                0);

            string title = _textProvider.Get("MarkTaskAs", _textProvider.Get("Completed"));
            builder.AddAction(Resource.Drawable.ic_check_white_48dp, title, mcPendingIntent);

            var notif = builder.Build();
            NotifManager.Notify(notification.Id, notif);
        }

        public void ShowNotification(TaskNotification notification)
        {
            //This avoid replacing an existing notification
            if (notification.Id == 0)
            {
                notification.Id  = (int)DateTime.Now.Ticks;
            }
            var builder = BuildSimpleNotification(notification);
            var notif = builder.Build();
            NotifManager.Notify(notification.Id, notif);
        }

        public void ShowNotification(int id, string title, string content, bool ongoing)
        {
            var pendingIntent = GetIntentToMainActivity(id);

            var notif = BuildSimpleNotification(title, content)
                .SetOngoing(ongoing)
                .SetContentIntent(pendingIntent)
                .Build();
            NotifManager.Notify(id, notif);
        }

        public NotificationCompat.Builder BuildSimpleNotification(string title, string content)
        {
            var iconDrawable = ContextCompat.GetDrawable(Application.Context, Resource.Drawable.ic_launcher);
            var bm = iconDrawable.ToBitmap();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var generalChannel = new NotificationChannel(GeneralChannelId, "Notifications", NotificationImportance.Default)
                {
                    LightColor = Resource.Color.colorAccent,
                    Description = "App notifications with sound"
                };

                generalChannel.EnableLights(true);
                NotifManager.CreateNotificationChannel(generalChannel);
            }

            var builder = new NotificationCompat.Builder(Application.Context, GeneralChannelId)
                .SetContentTitle(title)
                .SetSmallIcon(Resource.Drawable.ic_notification_logo)
                .SetColor(Color.Red.ToArgb())
                .SetLargeIcon(bm)
                .SetContentText(content);

            if (!string.IsNullOrEmpty(content))
                builder.SetContentText(content);

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                //This are deprecated for android O +
#pragma warning disable CS0618 // Type or member is obsolete
                var soundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
                builder.SetSound(soundUri);
                builder.SetPriority(NotificationCompat.PriorityDefault);
#pragma warning restore CS0618 // Type or member is obsolete
            }

            return builder;
        }

        public PendingIntent GetIntentToMainActivity(int notificationId)
        {
            //we use the main because we dont want to show the splash again..
            var resultIntent = new Intent(Application.Context, typeof(MainActivity));
            var bundle = new Bundle();
            bundle.PutString(nameof(NotificationService), $"{notificationId}");
            var pendingIntent = TaskStackBuilder.Create(Application.Context)
                .AddNextIntent(resultIntent)
                .GetPendingIntent(1, (int)PendingIntentFlags.UpdateCurrent, bundle);

            return pendingIntent;
        }

        private NotificationCompat.Builder BuildSimpleNotification(TaskNotification notification)
        {
            var pendingIntent = GetIntentToMainActivity(notification.Id);

            var builder = BuildSimpleNotification(notification.Title, notification.Content)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);

            if (!string.IsNullOrEmpty(notification.LargeContent))
            {
                builder.SetStyle(new NotificationCompat.BigTextStyle().BigText(notification.LargeContent));
            }

            return builder;
        }

        private AlarmManager GetAlarmManager()
        {
            var alarmManager = Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;
            return alarmManager;
        }
    }
}