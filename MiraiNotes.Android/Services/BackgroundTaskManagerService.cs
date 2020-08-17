using Android.OS;
using AndroidX.Work;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Background;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MvvmCross;
using MvvmCross.Platforms.Android;
using System;
using System.Threading.Tasks;
using Android.App;

namespace MiraiNotes.Android.Services
{
    public class BackgroundTaskManagerService : IBackgroundTaskManagerService
    {
        private readonly IAppSettingsService _appSettings;
        private readonly IDialogService _dialogService;
        private readonly ITextProvider _textProvider;

        private const int SyncId = 1;

        public BackgroundTaskManagerService(
            IAppSettingsService appSettings,
            IDialogService dialogService,
            ITextProvider textProvider)
        {
            _appSettings = appSettings;
            _dialogService = dialogService;
            _textProvider = textProvider;
        }

        public void RegisterBackgroundTasks(BackgroundTaskType backgroundTask, bool restart = true)
        {
            switch (backgroundTask)
            {
                case BackgroundTaskType.SYNC:
                    RegisterTask(SyncId, (long)_appSettings.SyncBackgroundTaskInterval);
                    break;
                case BackgroundTaskType.MARK_AS_COMPLETED:
                case BackgroundTaskType.ANY:
                default:
                    throw new NotSupportedException("The bg task is not supported");
            }
        }

        public async void StartBackgroundTask(BackgroundTaskType backgroundTask)
        {
            await Task.Delay(1);
            StartBackgroundTask(backgroundTask, null);
        }

        public async void StartBackgroundTask(BackgroundTaskType backgroundTask, BackgroundTaskParameter parameter)
        {
            switch (backgroundTask)
            {
                case BackgroundTaskType.SYNC:
                    await Task.Delay(10);
                    var top = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
                    var bundle = new Bundle();
                    if (parameter != null)
                    {
                        bundle.PutInt(SyncBackgroundService.TaskListIdToSyncKey, parameter.SyncOnlyTaskListId);
                    }
                    bundle.PutBoolean(SyncBackgroundService.StartedManuallyKey, true);
                    top.Activity.StartForegroundServiceCompat<SyncBackgroundService>(bundle);
                    break;
                case BackgroundTaskType.ANY:
                case BackgroundTaskType.MARK_AS_COMPLETED:
                default:
                    throw new NotSupportedException("The bg task is not supported");
            }
        }

        public void UnregisterBackgroundTasks(BackgroundTaskType backgroundTask = BackgroundTaskType.ANY)
        {
            switch (backgroundTask)
            {
                case BackgroundTaskType.ANY:
                case BackgroundTaskType.SYNC:
                    WorkManager.GetInstance(Application.Context).CancelAllWorkByTag($"{SyncId}");
                    break;
                case BackgroundTaskType.MARK_AS_COMPLETED:
                default:
                    throw new NotSupportedException("The bg task is not supported");
            }
        }

        private void RegisterTask(int id, long runEach)
        {
            var constraints = new Constraints.Builder()
                .SetRequiredNetworkType(NetworkType.Connected)
                .Build();
            var workRequest = PeriodicWorkRequest.Builder
                .From<SyncBackgroundTask>(TimeSpan.FromMinutes(runEach))
                .SetConstraints(constraints)
                .AddTag($"{id}")
                .Build();
            WorkManager.GetInstance(Application.Context).EnqueueUniquePeriodicWork($"{id}", ExistingPeriodicWorkPolicy.Replace, workRequest);
            _dialogService.ShowSucceedToast(_textProvider.Get("JobWasScheduled"));
        }
    }
}