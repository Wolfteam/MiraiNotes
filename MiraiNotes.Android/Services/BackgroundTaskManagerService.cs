using Android.App.Job;
using Android.Content;
using Java.Lang;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Background;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MvvmCross;
using MvvmCross.Platforms.Android;
using System;

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
                    long interval = (long)TimeSpan.FromMinutes((double)_appSettings.SyncBackgroundTaskInterval).TotalMilliseconds;
                    RegisterTask(SyncId, interval);
                    break;
                case BackgroundTaskType.MARK_AS_COMPLETED:
                case BackgroundTaskType.ANY:
                default:
                    throw new NotSupportedException("The bg task is not supported");
            }
        }

        public async void StartBackgroundTask(BackgroundTaskType backgroundTask)
        {
            switch (backgroundTask)
            {
                case BackgroundTaskType.SYNC:
                    using (var syncTask = new SyncBackgroundTask.SyncTask(null))
                    {
                        await syncTask.Sync();
                    }
                    break;
                case BackgroundTaskType.ANY:
                case BackgroundTaskType.MARK_AS_COMPLETED:
                default:
                    throw new NotSupportedException("The bg task is not supported");
            }
        }

        public void UnregisterBackgroundTasks(BackgroundTaskType backgroundTask = BackgroundTaskType.ANY)
        {
            var top = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            var scheduler = (JobScheduler)top.Activity.GetSystemService(Context.JobSchedulerService);
            switch (backgroundTask)
            {
                case BackgroundTaskType.ANY:
                case BackgroundTaskType.SYNC:
                    scheduler.Cancel(SyncId);
                    break;
                case BackgroundTaskType.MARK_AS_COMPLETED:
                default:
                    throw new NotSupportedException("The bg task is not supported");
            }
        }

        private void RegisterTask(int id, long? runEach)
        {
            var top = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            var component = new ComponentName(top.Activity, Class.FromType(typeof(SyncBackgroundTask)));
            var scheduler = (JobScheduler)top.Activity.GetSystemService(Context.JobSchedulerService);

            //keep in mind that if the job id is already registered, it will be updated
            var builder = new JobInfo.Builder(id, component)
                .SetRequiredNetworkType(NetworkType.Any)
                .SetPersisted(true)
                .SetRequiresBatteryNotLow(true);

            if (runEach.HasValue && runEach.Value > 0)
                builder.SetPeriodic(runEach.Value);

            var scheduleResult = scheduler.Schedule(builder.Build());

            if (scheduleResult == JobScheduler.ResultSuccess)
                _dialogService.ShowSucceedToast(_textProvider.Get("JobWasScheduled"));
            else
                _dialogService.ShowErrorToast(_textProvider.Get("JobCouldntBeScheduled"));
        }
    }
}