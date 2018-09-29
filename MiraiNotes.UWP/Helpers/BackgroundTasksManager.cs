using Microsoft.Toolkit.Uwp.Helpers;
using MiraiNotes.UWP.BackgroundTasks;
using MiraiNotes.UWP.Models;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Background;

namespace MiraiNotes.UWP.Helpers
{
    public class BackgroundTasksManager
    {
        /// <summary>
        /// Registers a particular bg tasks specified by <paramref name="backgroundTask"/>
        /// </summary>
        /// <param name="backgroundTask">The background task to register</param>
        /// <param name="restart">Indicates if the registration of the bg task is mandatory(True by default)</param>
        public static void RegisterBackgroundTask(BackgroundTaskType backgroundTask, int bgTaskInterval = 0, bool restart = true)
        {
            string bgTaskName;
            Type bgTaskType;
            IBackgroundTrigger trigger;
            var conditions = new List<IBackgroundCondition>();

            switch (backgroundTask)
            {
                case BackgroundTaskType.ANY:
                    throw new ArgumentException("Is not allowed to register all bg tasks at the same time");
                case BackgroundTaskType.SYNC:
                    bgTaskName = nameof(SyncBackgroundTask);
                    bgTaskType = typeof(SyncBackgroundTask);
                    trigger = new TimeTrigger((uint)bgTaskInterval, false);
                    conditions.Add(new SystemCondition(SystemConditionType.FreeNetworkAvailable));
                    conditions.Add(new SystemCondition(SystemConditionType.InternetAvailable));
                    break;
                case BackgroundTaskType.MARK_AS_COMPLETED:
                    bgTaskName = nameof(MarkAsCompletedBackgroundTask);
                    bgTaskType = typeof(MarkAsCompletedBackgroundTask);
                    trigger = new ToastNotificationActionTrigger();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Provided bg task {backgroundTask} does not exists");
            }

            bool isBgTaskAlreadyRegistered = BackgroundTaskHelper.IsBackgroundTaskRegistered(bgTaskType);
            if (isBgTaskAlreadyRegistered && !restart)
                return;

            if (isBgTaskAlreadyRegistered)
                UnregisterBackgroundTask(backgroundTask);

            if (bgTaskInterval <= 0 && backgroundTask != BackgroundTaskType.MARK_AS_COMPLETED)
                return;

            BackgroundTaskHelper.Register(
                bgTaskName,
                trigger,
                false,
                true,
                conditions.ToArray());
        }

        /// <summary>
        /// Unregister a background task specified by <paramref name="backgroundTask"/>
        /// </summary>
        /// <param name="backgroundTask">The background task to unregister</param>
        public static void UnregisterBackgroundTask(BackgroundTaskType backgroundTask = BackgroundTaskType.ANY)
        {
            switch (backgroundTask)
            {
                case BackgroundTaskType.ANY:
                    BackgroundTaskHelper.Unregister(nameof(SyncBackgroundTask));
                    BackgroundTaskHelper.Unregister(nameof(MarkAsCompletedBackgroundTask));
                    break;
                case BackgroundTaskType.SYNC:
                    BackgroundTaskHelper.Unregister(nameof(SyncBackgroundTask));
                    break;
                case BackgroundTaskType.MARK_AS_COMPLETED:
                    BackgroundTaskHelper.Unregister(nameof(MarkAsCompletedBackgroundTask));
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"The provided BackgroundTaskType doesnt exists {backgroundTask}");
            }
        }
    }
}
