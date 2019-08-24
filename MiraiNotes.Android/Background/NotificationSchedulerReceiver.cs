using Android.Content;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Models;
using MvvmCross;
using Newtonsoft.Json;

namespace MiraiNotes.Android.Background
{
    [BroadcastReceiver(Enabled = true, Label = "Notification Broadcast Receiver")]
    public class NotificationSchedulerReceiver : BroadcastReceiver
    {
        public const string LocalNotificationKey = "LocalNotification";

        public override void OnReceive(Context context, Intent intent)
        {
            string extra = intent.GetStringExtra(LocalNotificationKey);
            var notif = JsonConvert.DeserializeObject<TaskNotification>(extra);
            var notifService = Mvx.IoCProvider.Resolve<INotificationService>();
            notifService.ShowNotification(notif);
        }
    }
}