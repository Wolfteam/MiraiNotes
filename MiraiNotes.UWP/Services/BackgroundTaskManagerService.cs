using MiraiNotes.UWP.BackgroundTasks;
using MiraiNotes.UWP.Helpers;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System;
using MiraiNotes.Abstractions.Services;

namespace MiraiNotes.UWP.Services
{
    public class BackgroundTaskManagerService : IBackgroundTaskManagerService
    {
        private readonly IAppSettingsService _appSettings;

        public BackgroundTaskManagerService(IAppSettingsService appSettings)
        {
            _appSettings = appSettings;
        }

        public void RegisterBackgroundTasks(BackgroundTaskType backgroundTask, bool restart = true)
        {
            int bgTaskInterval = 0;

            switch (backgroundTask)
            {
                case BackgroundTaskType.ANY:
                    throw new ArgumentException("Is not allowed to register all bg tasks at the same time");
                case BackgroundTaskType.SYNC:
                    bgTaskInterval = (int)_appSettings.SyncBackgroundTaskInterval;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(backgroundTask), backgroundTask, "The bg task cant be registered because it doesnt exits");
            }

            BackgroundTasksManager.RegisterBackgroundTask(backgroundTask, bgTaskInterval, restart);
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
