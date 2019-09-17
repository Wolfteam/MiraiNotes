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
using MiraiNotes.Android.Views.Activities;
using MiraiNotes.Core.Models;
using Newtonsoft.Json;
using System;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;

namespace MiraiNotes.Android.Services
{
    public class NotificationService : INotificationService
    {
        private static string PackageName
            => Application.Context.PackageName;

        public static string GeneralChannelId
            => $"{PackageName}.general";

        private NotificationManager NotifManager
            => (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);

        public void RemoveScheduledNotification(int id)
        {
            var intent = CreateSchedulerIntent(id);
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
            var localNotification = new TaskNotification
            {
                Conntent = 
                    $"{notification.TaskListTitle}{System.Environment.NewLine}" +
                    $"{notification.TaskTitle}{System.Environment.NewLine}" +
                    $"{notification.TaskBody}",
                Title = notification.TaskListTitle,
                Id = notification.Id
            };


            if (showNow)
            {
                ShowNotification(localNotification);
                return;
            }

            var intent = CreateSchedulerIntent(notification.Id);
            var serializedNotification = JsonConvert.SerializeObject(localNotification);
            intent.PutExtra(NotificationSchedulerReceiver.LocalNotificationKey, serializedNotification);

            var pendingIntent =
                PendingIntent.GetBroadcast(Application.Context, 0, intent, PendingIntentFlags.CancelCurrent);
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

        public void ShowNotification(TaskNotification notification)
        {
            //TODO: APP ICON
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

            var soundUri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);

            var audioAttributes = new AudioAttributes.Builder()
                .SetContentType(AudioContentType.Sonification)
                .SetUsage(AudioUsageKind.Alarm)
                .SetLegacyStreamType(Stream.Alarm)
                .Build();

            //we use the main because we dont want to show the splash again..
            var resultIntent = new Intent(Application.Context, typeof(MainActivity));
            //resultIntent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            //resultIntent.SetAction(Intent.ActionMain);
            //resultIntent.AddCategory(Intent.CategoryLauncher);
            //resultIntent.SetFlags(ActivityFlags.SingleTop);
            //resultIntent.SetFlags(ActivityFlags.NewTask);

            //var pendingIntent = PendingIntent.GetActivity(Application.Context, 1, resultIntent, PendingIntentFlags.UpdateCurrent);
            var bundle = new Bundle();
            bundle.PutString(nameof(NotificationService), $"{notification.Id}");
            var pendingIntent = TaskStackBuilder.Create(Application.Context)
                .AddNextIntent(resultIntent)
                .GetPendingIntent(1, (int)PendingIntentFlags.UpdateCurrent, bundle);

            var builder = new Notification.Builder(Application.Context, GeneralChannelId)
                .SetContentTitle(notification.Title)
                .SetContentText(notification.Conntent)
                //TODO: ACTIONS IN CASE OF SCHEDULED NOTIF
                .SetAutoCancel(true)
                //TODO: APPICON
                .SetSmallIcon(Resource.Drawable.ic_notification_logo)
                .SetColor(Color.Red.ToArgb())
                .SetLargeIcon(bm)
                .SetContentIntent(pendingIntent);

            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                //This are deprecated for android O +
#pragma warning disable CS0618 // Type or member is obsolete
                builder.SetSound(soundUri, audioAttributes);
                builder.SetPriority(NotificationCompat.PriorityDefault);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            var notif = builder.Build();

            NotifManager.Notify(notification.Id, notif);
        }

        private Intent CreateSchedulerIntent(int id)
        {
            return new Intent(Application.Context, typeof(NotificationSchedulerReceiver))
                .SetAction("LocalNotifierIntent" + id);
        }

        private AlarmManager GetAlarmManager()
        {
            var alarmManager = Application.Context.GetSystemService(Context.AlarmService) as AlarmManager;
            return alarmManager;
        }
    }
}