using Android.App;
using Android.Content;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Services;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Models;
using MiraiNotes.Shared.Helpers;
using MvvmCross;
using MvvmCross.Platforms.Android.Services;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.Background
{
    [BroadcastReceiver(Enabled = true, Label = "Boot receiver")]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted })]
    public class BootBroadcastReceiver : MvxBroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);
            using var thread = new RescheduleNotificationsTask();
            thread.Run();
        }

        public class RescheduleNotificationsTask : Java.Lang.Thread
        {
            private readonly ILogger _logger;
            private readonly ITelemetryService _telemetryService;
            private readonly INotificationService _notificationService;
            private readonly IMiraiNotesDataService _dataService;

            public RescheduleNotificationsTask()
            {
                _logger = Mvx.IoCProvider.Resolve<ILogger>().ForContext<RescheduleNotificationsTask>();
                _telemetryService = Mvx.IoCProvider.Resolve<ITelemetryService>();
                _notificationService = Mvx.IoCProvider.Resolve<INotificationService>() as NotificationService;
                _dataService = Mvx.IoCProvider.Resolve<IMiraiNotesDataService>();
            }

            public override async void Run()
            {
                base.Run();
                await SchedulePendingNotifications();
            }

            public async Task SchedulePendingNotifications()
            {
                try
                {

                    _logger.Information($"Getting tasks with a reminder date...");

                    var taskResponse = await _dataService.TaskService
                        .GetAsNoTrackingAsync(
                            t => t.RemindOnGUID != null && t.RemindOn.HasValue,
                            includeProperties: nameof(GoogleTask.TaskList));

                    if (!taskResponse.Succeed)
                    {
                        _logger.Warning(
                            $"Could not get all the task with a reminder date. " +
                            $"Error = {taskResponse.Message}");
                        return;
                    }

                    var tasks = taskResponse.Result;

                    _logger.Information($"Scheduling notifications for {tasks.Count()} task(s)");
                    foreach (var task in tasks)
                    {
                        string notes = TasksHelper.GetNotesForNotification(task.Notes);
                        int id = int.Parse(task.RemindOnGUID);
                        _notificationService.ScheduleNotification(new TaskReminderNotification
                        {
                            Id = id,
                            TaskListId = task.TaskList.ID,
                            TaskId = task.ID,
                            TaskListTitle = task.TaskList.Title,
                            TaskTitle = task.Title,
                            TaskBody = notes,
                            DeliveryOn = task.RemindOn.Value
                        });
                    }

                    _logger.Information($"Process completed");
                }
                catch (Exception e)
                {
                    _logger?.Error(e,
                        $"An unknown error occurred while trying " +
                        $"to schedule pending notifications");
                    _telemetryService?.TrackError(e);
                }
            }
        }
    }
}