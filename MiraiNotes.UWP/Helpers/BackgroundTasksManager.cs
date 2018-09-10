using Microsoft.Toolkit.Uwp.Helpers;
using MiraiNotes.UWP.BackgroundTasks;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Services;
using System;
using Windows.ApplicationModel.Background;

namespace MiraiNotes.UWP.Helpers
{
    public class BackgroundTasksManager
    {
        //TODO: NOT SURE IF I SHOULD REGISTER ALL MY BG TASKS AT THE SAME TIME
        public static void RegisterBackgroundTasks()
        {
            if (BackgroundTaskHelper.IsBackgroundTaskRegistered(typeof(SyncBackgroundTask)))
            {
                // Background task already registered.
                return;
            }

            //TODO: CHANGE THIS WHEN YOU IMPLEMENT APP SETTINGS 
            int syncBackgroundTaskInterval = ApplicationSettingsService.SyncBackgroundTaskInterval;
            if (syncBackgroundTaskInterval <= 0)
                return;

            //TODO: CHANGE THE DEFAULT TIME THAT THE BGTask WILL GET EXECUTED
            BackgroundTaskHelper.Register(
                nameof(SyncBackgroundTask),
                new TimeTrigger((uint)syncBackgroundTaskInterval, false),
                false,
                true,
                new SystemCondition(SystemConditionType.FreeNetworkAvailable),
                new SystemCondition(SystemConditionType.InternetAvailable));
        }

        /// <summary>
        /// Unregister a background task specified by <paramref name="backgroundTask"/>
        /// </summary>
        /// <param name="backgroundTask">The background task to unregister</param>
        public static void UnregisterBackgroundTasks(BackgroundTaskType backgroundTask = BackgroundTaskType.ANY)
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
