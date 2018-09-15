using Microsoft.Toolkit.Uwp.Helpers;
using MiraiNotes.UWP.BackgroundTasks;
using MiraiNotes.UWP.Models;
using System;
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

            switch (backgroundTask)
            {
                case BackgroundTaskType.ANY:
                    throw new ArgumentException("Is not allowed to register all bg tasks at the same time");
                case BackgroundTaskType.SYNC:
                    bgTaskName = nameof(SyncBackgroundTask);
                    bgTaskType = typeof(SyncBackgroundTask);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Provided bg task {backgroundTask} does not exists");
            }

            bool isBgTaskAlreadyRegistered = BackgroundTaskHelper.IsBackgroundTaskRegistered(bgTaskType);
            if (isBgTaskAlreadyRegistered && !restart)
                return;

            if (isBgTaskAlreadyRegistered)
                UnregisterBackgroundTask(backgroundTask);

            if (bgTaskInterval <= 0)
                return;

            BackgroundTaskHelper.Register(
                bgTaskName,
                new TimeTrigger((uint)bgTaskInterval, false),
                false,
                true,
                new SystemCondition(SystemConditionType.FreeNetworkAvailable),
                new SystemCondition(SystemConditionType.InternetAvailable));
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
                    break;
                case BackgroundTaskType.SYNC:
                    BackgroundTaskHelper.Unregister(nameof(SyncBackgroundTask));
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"The provided BackgroundTaskType doesnt exists {backgroundTask}");
            }
        }
    }
}
