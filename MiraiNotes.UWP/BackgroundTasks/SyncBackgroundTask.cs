using GalaSoft.MvvmLight.Messaging;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Models;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.ViewModels;
using Serilog;
using System;
using System.Linq;
using Windows.ApplicationModel.Background;

namespace MiraiNotes.UWP.BackgroundTasks
{
    public sealed class SyncBackgroundTask : IBackgroundTask
    {
        private readonly IAppSettingsService _appSettings;
        private readonly ISyncService _syncService;
        private readonly ILogger _logger;
        private readonly IMessenger _messenger;
        private readonly INotificationService _notificationService;
        private readonly bool _isAppRunning;
        private readonly int? _taskListId;
        private BackgroundTaskDeferral _deferral;

        public SyncBackgroundTask(int? taskListId)
            : this()
        {
            _taskListId = taskListId;
        }

        public SyncBackgroundTask()
        {
            _isAppRunning = ViewModelLocator.IsAppRunning;
            var vml = new ViewModelLocator();

            _appSettings = vml.AppSettingsService;
            _syncService = vml.SyncService;
            _logger = vml.Logger.ForContext<SyncBackgroundTask>();
            _messenger = vml.Messenger;
            _notificationService = vml.NotificationService;

            if (_isAppRunning)
                _logger.Information($"{nameof(SyncBackgroundTask)}: is being started when the app is already running");
            else
                _logger.Information($"{nameof(SyncBackgroundTask)}: is being started when the app is not running");
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //TODO: THIS BG TASK COULD RUN FOR MORE THAN 30 SEC...
            bool startedManually = taskInstance is null;
            if (!startedManually)
                taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            _deferral = taskInstance?.GetDeferral();
            _logger.Information($"{nameof(SyncBackgroundTask)}: Started {(startedManually ? "manually" : "automatically")}");

            if (_isAppRunning)
            {
                _messenger.Send(false, $"{MessageType.OPEN_PANE}");
                _messenger.Send(
                    new Tuple<bool, string>(
                        true,
                        $"Performing a {(startedManually ? "manual" : "automatic")} full sync, please wait..."),
                    $"{MessageType.SHOW_MAIN_PROGRESS_BAR}");
            }

            bool syncOnlyOneTaskList = _taskListId.HasValue;
            if (syncOnlyOneTaskList)
                _logger.Information($"{nameof(SyncBackgroundTask)}: We will perform a partial sync for the " +
                    $"tasklistId = {_taskListId.Value} and its associated tasks");
            else
                _logger.Information($"{nameof(SyncBackgroundTask)}: We will perform a full sync");

            var syncResults = !syncOnlyOneTaskList
                ? await _syncService.PerformFullSync(true)
                : await _syncService.PerformSyncOnlyOn(_taskListId.Value);

            string message = syncResults.Any(r => !r.Succeed) ?
                string.Join($".{Environment.NewLine}", syncResults.Where(r => !r.Succeed).Select(r => r.Message).Distinct()) :
                $"A {(startedManually ? "manual" : "automatic")} full sync was successfully performed.";

            if (string.IsNullOrEmpty(message))
                message = "An unknown error occurred while trying to perform the sync operation.";

            _logger.Information($"{nameof(SyncBackgroundTask)}: results = {message}");

            if (!_isAppRunning && !startedManually && _appSettings.ShowToastNotificationAfterFullSync)
            {
                _notificationService.ShowNotification(new TaskNotification
                {
                    Title = "Sync Results",
                    Content = message
                });
            }
            else
            {
                _messenger.Send(message, $"{MessageType.SHOW_IN_APP_NOTIFICATION}");
                _messenger.Send(new Tuple<bool, string>(false, null), $"{MessageType.SHOW_MAIN_PROGRESS_BAR}");
                _messenger.Send(true, $"{MessageType.ON_FULL_SYNC}");
            }

            _logger.Information($"{nameof(SyncBackgroundTask)}: completed successfully");
            _deferral?.Complete();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //TODO: I SHOULD CANCEL THE BG TASKS HERE
            _logger.Warning($"{sender.Task.Name} cancel requested... Cancel reason = {reason.ToString()}");
        }
    }
}
