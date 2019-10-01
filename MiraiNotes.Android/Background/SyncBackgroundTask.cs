using Android.App;
using Android.App.Job;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Models;
using MvvmCross;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.Background
{
    [Service(Permission = "android.permission.BIND_JOB_SERVICE")]
    public class SyncBackgroundTask : JobService
    {
        private JobParameters _params;

        public override bool OnStartJob(JobParameters @params)
        {
            _params = @params;
            using (var thread = new SyncTask(this))
            {
                thread.Run();
            }
            // Return true because of the asynchronous work
            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            // we don't want to reschedule the job if it is stopped or cancelled.
            return false;
        }

        public class SyncTask : Java.Lang.Thread
        {
            private readonly SyncBackgroundTask _parentBgTask;
            public SyncTask(SyncBackgroundTask parent)
            {
                _parentBgTask = parent;
            }

            public override async void Run()
            {
                base.Run();

                await Sync();
            }

            public async Task Sync()
            {
                try
                {
                    bool startedManually = _parentBgTask == null;
                    bool isAppInForeground = AndroidUtils.IsAppInForeground();
                    if (!isAppInForeground)
                    {
                        new App().Initialize();
                    }

                    var syncService = Mvx.IoCProvider.Resolve<ISyncService>();
                    var logger = Mvx.IoCProvider.Resolve<ILogger>().ForContext<SyncBackgroundTask>();
                    var textProvider = Mvx.IoCProvider.Resolve<ITextProvider>();
                    var appSettings = Mvx.IoCProvider.Resolve<IAppSettingsService>();
                    var messenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();
                    var dialogService = Mvx.IoCProvider.Resolve<IDialogService>();
                    var notificationService = Mvx.IoCProvider.Resolve<INotificationService>();

                    logger.Information(
                        $"{nameof(SyncBackgroundTask)}: Started {(startedManually ? "manually" : "automatically")}");

                    if (isAppInForeground)
                    {
                        messenger.Publish(new ShowProgressOverlayMsg(this));
                    }

                    var syncResults = new List<EmptyResponseDto>
                    {
                        await syncService.SyncDownTaskListsAsync(true),
                        await syncService.SyncDownTasksAsync(true),
                        await syncService.SyncUpTaskListsAsync(true),
                        await syncService.SyncUpTasksAsync(true)
                    };

                    if (syncResults.Any(r => !r.Succeed))
                    {
                        var errors = string.Join(
                            $".{Environment.NewLine}",
                            syncResults
                                .Where(r => !r.Succeed)
                                .Select(r => r.Message)
                                .Distinct());

                        logger.Error($"{nameof(SyncBackgroundTask)}: Sync completed with errors = {errors}");
                    }

                    string message = syncResults.Any(r => !r.Succeed) ?
                        textProvider.Get("SyncUnknownError") :
                        !startedManually
                            ? textProvider.Get("SyncAutoCompleted")
                            : textProvider.Get("SyncManualCompleted");

                    if (string.IsNullOrEmpty(message))
                        message = "An unknown error occurred while trying to perform the sync operation.";

                    logger.Information($"{nameof(SyncBackgroundTask)}: results = {message}");

                    if (!isAppInForeground
                        && !startedManually
                        && appSettings.ShowToastNotificationAfterFullSync)
                    {
                        logger.Information($"{nameof(SyncBackgroundTask)}: App is not in foreground, showing a notification...");
                        var notif = new TaskNotification
                        {
                            Content = message,
                            Title = textProvider.Get("SyncResults")
                        };
                        notificationService.ShowNotification(notif);
                    }
                    else
                    {
                        logger.Information($"{nameof(SyncBackgroundTask)}: App is in foreground, showing the snackbar msg...");
                        dialogService.ShowSnackBar(message);
                        messenger.Publish(new ShowProgressOverlayMsg(this, false));
                        messenger.Publish(new OnFullSyncMsg(this));
                    }

                    logger.Information($"{nameof(SyncBackgroundTask)}: completed successfully");
                }
                catch (Exception e)
                {
                    //well
                }
                finally
                {
                    _parentBgTask?.JobFinished(_parentBgTask?._params, false);
                }
            }
        }
    }
}