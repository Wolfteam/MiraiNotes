using Android.App;
using Android.Content;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MvvmCross;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MiraiNotes.Android.Background
{
    [BroadcastReceiver(Enabled = true, Label = "Mark task as completed broadcast receiver")]
    public class MarkTaskAsCompletedReceiver : BroadcastReceiver
    {
        public const string MarkTaskAsCompletedKey = "MarkTaskAsCompleted";
        public override void OnReceive(Context context, Intent intent)
        {
            string extra = intent.GetStringExtra(MarkTaskAsCompletedKey);
            var notif = JsonConvert.DeserializeObject<TaskReminderNotification>(extra);
            using (var thread = new MarkAsCompletedTask(notif))
            {
                thread.Run();
            };

            var manager = (NotificationManager)context.GetSystemService(Context.NotificationService);
            manager.Cancel(notif.Id);
        }

        //the setaction method is required, otherwise the extras gets loss  
        //https://stackoverflow.com/questions/3168484/pendingintent-works-correctly-for-the-first-notification-but-incorrectly-for-the
        public static Intent CreateIntent(int id, string key, string extra) =>
            new Intent(Application.Context, typeof(MarkTaskAsCompletedReceiver))
                .SetAction(nameof(MarkTaskAsCompletedReceiver) + id)
                .PutExtra(key, extra);

        public class MarkAsCompletedTask : Java.Lang.Thread
        {
            private readonly TaskReminderNotification _notification;

            public MarkAsCompletedTask(TaskReminderNotification notification)
            {
                _notification = notification;
            }

            public override async void Run()
            {
                base.Run();

                await MarkAsCompleted();
            }

            public async Task MarkAsCompleted()
            {
                try
                {
                    bool isAppInForeground = AndroidUtils.IsAppInForeground();
                    if (!isAppInForeground)
                    {
                        new App().Initialize();
                    }

                    var dataService = Mvx.IoCProvider.Resolve<IMiraiNotesDataService>();
                    var logger = Mvx.IoCProvider.Resolve<ILogger>().ForContext<MarkTaskAsCompletedReceiver>();
                    var notificationService = Mvx.IoCProvider.Resolve<INotificationService>();

                    logger.Information($"Marking taskId = {_notification.TaskId} as completed...");

                    var response = await dataService.TaskService.ChangeTaskStatusAsync(
                        _notification.TaskId,
                        GoogleTaskStatus.COMPLETED);

                    if (response.Succeed)
                    {
                        logger.Information($"TaskId = {_notification.TaskId} was successfully marked as completed");
                    }
                    else
                    {
                        logger.Error($"TaskId = {_notification.TaskId} couldnt be marked as completed. Error = {response.Message}");

                        //TRANSLATE THIS
                        notificationService.ShowNotification(new TaskNotification
                        {
                            Content = $"Task {_notification.TaskTitle} couldnt be marked as completed",
                            Title = "Error"
                        });
                    }
                }
                catch (Exception)
                {
                    //well...
                }
            }
        }
    }
}