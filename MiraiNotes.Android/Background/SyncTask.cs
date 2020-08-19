using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Models;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.Background
{
    public class SyncTask
    {
        private readonly bool _startedManually;
        private readonly int? _taskListId;
        private readonly ISyncService _syncService;
        private readonly IMvxMainThreadAsyncDispatcher _dispatcher;
        private readonly ILogger _logger;
        private readonly ITextProvider _textProvider;
        private readonly IAppSettingsService _appSettings;
        private readonly IAndroidAppSettings _androidAppSettings;
        private readonly IMvxMessenger _messenger;
        private readonly IDialogService _dialogService;
        private readonly IAndroidNotificationService _notificationService;
        private readonly ITelemetryService _telemetryService;
        private readonly INetworkService _networkService;

        private const string IsServiceRunningKey = "IsSyncServiceRunning";

        public SyncTask(bool startedManually, int? taskListId)
        {
            _startedManually = startedManually;
            _taskListId = taskListId;

            _dispatcher = Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>();
            _syncService = Mvx.IoCProvider.Resolve<ISyncService>();
            _logger = Mvx.IoCProvider.Resolve<ILogger>().ForContext<SyncBackgroundTask>();
            _textProvider = Mvx.IoCProvider.Resolve<ITextProvider>();
            _appSettings = Mvx.IoCProvider.Resolve<IAppSettingsService>();
            _androidAppSettings = _appSettings as IAndroidAppSettings;
            _messenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();
            _dialogService = Mvx.IoCProvider.Resolve<IDialogService>();
            _notificationService = Mvx.IoCProvider.Resolve<IAndroidNotificationService>();
            _telemetryService = Mvx.IoCProvider.Resolve<ITelemetryService>();
            _networkService = Mvx.IoCProvider.Resolve<INetworkService>();
        }

        public async Task Sync()
        {
            try
            {
                _logger.Information(
                    $"{nameof(Sync)}: Started {(_startedManually ? "manually" : "automatically")}");

                bool isSyncServiceRunning = _androidAppSettings.GetBoolean(IsServiceRunningKey);

                if (!_startedManually && isSyncServiceRunning)
                {
                    _logger.Warning($"{nameof(Sync)}: The sync bg service is already running...");
                    return;
                }

                _androidAppSettings.SetBoolean(IsServiceRunningKey, true);

                string msg = $"{_textProvider.Get("Syncing")}...";
                await _dispatcher.ExecuteOnMainThreadAsync(() => _messenger.Publish(new ShowProgressOverlayMsg(this, msg: msg)));

                if (!_networkService.IsInternetAvailable())
                {
                    _logger.Warning($"{nameof(Sync)}: Network is not available...");
                    msg = _textProvider.Get("NetworkNotAvailable");
                    await _dispatcher.ExecuteOnMainThreadAsync(() => _messenger.Publish(new ShowProgressOverlayMsg(this, false, msg)));
                    return;
                }

                bool syncOnlyOneTaskList = _taskListId.HasValue;
                if (syncOnlyOneTaskList)
                    _logger.Information($"{nameof(Sync)}: We will perform a partial sync for the " +
                                        $"tasklistId = {_taskListId.Value} and its associated tasks");
                else
                    _logger.Information($"{nameof(Sync)}: We will perform a full sync");

                var syncResults = !syncOnlyOneTaskList
                    ? await _syncService.PerformFullSync(true)
                    : await _syncService.PerformSyncOnlyOn(_taskListId.Value);

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

                string message = syncResults.Any(r => !r.Succeed)
                    ? _textProvider.Get("SyncUnknownError")
                    : !_startedManually
                        ? _textProvider.Get("SyncAutoCompleted")
                        : _textProvider.Get("SyncManualCompleted");

                if (string.IsNullOrEmpty(message))
                    message = "An unknown error occurred while trying to perform the sync operation.";

                _logger.Information($"{nameof(Sync)}: results = {message}");
                await _dispatcher.ExecuteOnMainThreadAsync(() =>
                    _messenger.Publish(new ShowProgressOverlayMsg(this, false)));

                bool isInForeground = AndroidUtils.IsAppInForeground();
                if (!isInForeground && _appSettings.ShowToastNotificationAfterFullSync)
                {
                    _logger.Information($"{nameof(Sync)}: App is not in foreground, showing a notification...");
                    var notification = new TaskNotification
                    {
                        Content = message,
                        Title = _textProvider.Get("SyncResults")
                    };
                    _notificationService.ShowNotification(notification);
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
                _telemetryService?.TrackError(e);
            }
            finally
            {
                _androidAppSettings.SetBoolean(IsServiceRunningKey, false);
            }
        }
    }
}