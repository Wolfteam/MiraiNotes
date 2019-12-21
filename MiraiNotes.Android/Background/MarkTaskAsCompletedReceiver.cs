using Android.App;
using Android.Content;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MvvmCross;
using MvvmCross.Plugin.Messenger;
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

            private ILogger _logger;
            private ITelemetryService _telemetryService;

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
                    var notificationService = Mvx.IoCProvider.Resolve<INotificationService>();
                    var textProvider = Mvx.IoCProvider.Resolve<ITextProvider>();
                    var messenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();
                    _logger = Mvx.IoCProvider.Resolve<ILogger>().ForContext<MarkAsCompletedTask>();
                    _telemetryService = Mvx.IoCProvider.Resolve<ITelemetryService>();

                    if (!isAppInForeground)
                        _telemetryService.Init();

                    _logger.Information($"Marking taskId = {_notification.TaskId} as completed...");
                    //if i pass the notifaction in the lambda, the task crashes..
                    int taskId = _notification.TaskId;
                    var taskResponse = await dataService.TaskService
                        .FirstOrDefaultAsNoTrackingAsync(t => t.ID == taskId);
                    if (!taskResponse.Succeed)
                    {
                        _logger.Error($"Couldnt retrieve taskId = {_notification.Id}. Error = {taskResponse.Message}");
                        return;
                    }
                    var googleTaskId = taskResponse.Result.GoogleTaskID;
                    var response = await dataService.TaskService
                        .ChangeTaskStatusAsync(googleTaskId, GoogleTaskStatus.COMPLETED);

                    if (response.Succeed)
                    {
                        _logger.Information($"TaskId = {_notification.TaskId} was successfully marked as completed");

                        if (isAppInForeground)
                        {
                            var task = response.Result;
                            var msg = new TaskStatusChangedMsg(
                                this,
                                task.GoogleTaskID,
                                task.ParentTask,
                                task.CompletedOn,
                                task.UpdatedAt,
                                task.Status);
                            messenger.Publish(msg);
                        }
                    }
                    else
                    {
                        _logger.Error($"TaskId = {_notification.TaskId} couldnt be marked as completed. Error = {response.Message}");

                        notificationService.ShowNotification(new TaskNotification
                        {
                            Content = textProvider.Get("TaskCouldntBeMarkedAsCompleted", _notification.TaskTitle),
                            Title = textProvider.Get("Error")
                        });
                    }

                    _logger.Information("Process completed...");
                }
                catch (Exception e)
                {
                    _logger?.Error(e, $"An unknown error occurred while trying to mark taskId = {_notification.Id} as completed");
                    _telemetryService?.TrackError(e);
                }
            }
        }
    }
}