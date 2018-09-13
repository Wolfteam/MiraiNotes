using MiraiNotes.UWP.BackgroundTasks;
using MiraiNotes.UWP.Helpers;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System;

namespace MiraiNotes.UWP.Services
{
    public class BackgroundTaskManagerService : IBackgroundTaskManagerService
    {
        public void RegisterBackgroundTasks(BackgroundTaskType backgroundTask, bool restart = true)
        {
            BackgroundTasksManager.RegisterBackgroundTask(backgroundTask, restart);
        }

        public void StartBackgroundTask(BackgroundTaskType backgroundTask)
        {
            //TODO: HANDLE MULTIPLE BG TASK START, YOU COULD DO IT BY USING APP SETTINGS
            switch (backgroundTask)
            {
                case BackgroundTaskType.ANY:
                    throw new ArgumentOutOfRangeException("Its not allowed to start all the bg tasks at the same time");
                case BackgroundTaskType.SYNC:
                    new SyncBackgroundTask().Run(null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Cant start the provided bg task = {backgroundTask}");
            }
        }

        public void UnregisterBackgroundTasks(BackgroundTaskType backgroundTask = BackgroundTaskType.ANY)
        {
            BackgroundTasksManager.UnregisterBackgroundTask(backgroundTask);
        }
    }
}
