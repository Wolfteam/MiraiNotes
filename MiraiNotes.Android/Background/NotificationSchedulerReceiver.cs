using Android.App;
using Android.Content;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Services;
using MiraiNotes.Core.Models;
using MvvmCross;
using Newtonsoft.Json;

namespace MiraiNotes.Android.Background
{
    [BroadcastReceiver(Enabled = true, Label = "Notification Broadcast Receiver")]
    public class NotificationSchedulerReceiver : BroadcastReceiver
    {
        public const string LocalNotificationKey = "LocalNotification";
        public const string TaskReminderNotificationKey = "TaskReminderNotification";

        public override void OnReceive(Context context, Intent intent)
        {
            string extraA = intent.GetStringExtra(LocalNotificationKey);
            string extraB = intent.GetStringExtra(TaskReminderNotificationKey);
            var notifService = Mvx.IoCProvider.Resolve<INotificationService>() as NotificationService;

            if (string.IsNullOrEmpty(extraB))
            {
                var notif = JsonConvert.DeserializeObject<TaskNotification>(extraA);
                notifService.ShowNotification(notif);
            }
            else
            {
                var notif = JsonConvert.DeserializeObject<TaskReminderNotification>(extraB);
                notifService.ShowTaskReminderNotification(notif);
            }
        }

        //the secaction method is required, otherwise the extras gets loss  
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
            
    }
}