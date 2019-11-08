using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Models;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.Background
{
    public class SyncTask
    {
        private readonly bool _startedManually;
        private ISyncService _syncService;
        private IMvxMainThreadAsyncDispatcher _dispatcher;
        private ILogger _logger;
        private ITextProvider _textProvider;
        private IAppSettingsService _appSettings;
        private IMvxMessenger _messenger;
        private IDialogService _dialogService;
        private INotificationService _notificationService;

        public SyncTask(bool startedManually)
        {
            _startedManually = startedManually;
        }

        public async Task Sync()
        {
            try
            {
                if (!AndroidUtils.IsAppInForeground())
                {
                    new App().Initialize();
                }
                _dispatcher = Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>();
                _syncService = Mvx.IoCProvider.Resolve<ISyncService>();
                _logger = Mvx.IoCProvider.Resolve<ILogger>().ForContext<SyncBackgroundTask>();
                _textProvider = Mvx.IoCProvider.Resolve<ITextProvider>();
                _appSettings = Mvx.IoCProvider.Resolve<IAppSettingsService>();
                _messenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();
                _dialogService = Mvx.IoCProvider.Resolve<IDialogService>();
                _notificationService = Mvx.IoCProvider.Resolve<INotificationService>();

                _logger.Information(
                    $"{nameof(Sync)}: Started {(_startedManually ? "manually" : "automatically")}");

                bool isSyncServiceRunning = (_appSettings as IAndroidAppSettings).GetBoolean(SyncBackgroundService.IsServiceRunningKey);

                if (!_startedManually && isSyncServiceRunning)
                {
                    _logger.Warning($"{nameof(Sync)}: The sync bg service is already running...");
                    return;
                }

                await _dispatcher.ExecuteOnMainThreadAsync(() => _messenger.Publish(new ShowProgressOverlayMsg(this)));

                var syncResults = new List<EmptyResponseDto>
                {
                    await _syncService.SyncDownTaskListsAsync(true),
                    await _syncService.SyncDownTasksAsync(true),
                    await _syncService.SyncUpTaskListsAsync(true),
                    await _syncService.SyncUpTasksAsync(true)
                };

                if (syncResults.Any(r => !r.Succeed))
                {
                    var errors = string.Join(
                        $".{Environment.NewLine}",
                        syncResults
                            .Where(r => !r.Succeed)
                            .Select(r => r.Message)
                            .Distinct());

                    _logger.Error($"{nameof(Sync)}: Sync completed with errors = {errors}");
                }

                string message = syncResults.Any(r => !r.Succeed) ?
                    _textProvider.Get("SyncUnknownError") :
                    !_startedManually
                        ? _textProvider.Get("SyncAutoCompleted")
                        : _textProvider.Get("SyncManualCompleted");

                if (string.IsNullOrEmpty(message))
                    message = "An unknown error occurred while trying to perform the sync operation.";

                _logger.Information($"{nameof(Sync)}: results = {message}");
                await _dispatcher.ExecuteOnMainThreadAsync(() => _messenger.Publish(new ShowProgressOverlayMsg(this, false)));

                bool isInForeground = AndroidUtils.IsAppInForeground();
                if (!isInForeground && _appSettings.ShowToastNotificationAfterFullSync)
                {
                    _logger.Information($"{nameof(Sync)}: App is not in foreground, showing a notification...");
                    var notif = new TaskNotification
                    {
                        Content = message,
                        Title = _textProvider.Get("SyncResults")
                    };
                    _notificationService.ShowNotification(notif);
                }
                else if (isInForeground)
                {
                    _logger.Information($"{nameof(Sync)}: App is in foreground, showing the snackbar msg...");
                    await _dispatcher.ExecuteOnMainThreadAsync(() =>
                    {
                        _dialogService.ShowSnackBar(message);
                        _messenger.Publish(new OnFullSyncMsg(this));
                    });
                }

                _logger.Information($"{nameof(Sync)}: completed successfully");
            }
            catch (Exception e)
            {
                _logger?.Error(e, $"{nameof(Sync)}: An unknown error occurred while trying to sync task / tasklists");
            }
        }
    }
}