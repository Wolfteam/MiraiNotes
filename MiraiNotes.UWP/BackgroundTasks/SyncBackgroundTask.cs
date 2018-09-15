using GalaSoft.MvvmLight.Messaging;
using Microsoft.Toolkit.Uwp.Notifications;
using MiraiNotes.Shared.Models;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace MiraiNotes.UWP.BackgroundTasks
{
    public sealed class SyncBackgroundTask : IBackgroundTask
    {
        private readonly IApplicationSettingsService _appSettings;
        private readonly ISyncService _syncService;
        private readonly ILogger _logger;
        private readonly IMessenger _messenger;
        private readonly bool _isAppRunning;
        private BackgroundTaskDeferral _deferral;

        public SyncBackgroundTask()
        {
            _isAppRunning = ViewModelLocator.IsAppRunning;
            var vml = new ViewModelLocator();

            _appSettings = vml.ApplicationSettingsService;
            _syncService = vml.SyncService;
            _logger = vml.Logger.ForContext<SyncBackgroundTask>();
            _messenger = vml.Messenger;
            
            if (_isAppRunning)
                _logger.Information($"{nameof(SyncBackgroundTask)} is being started when the app is already running");
            else
                _logger.Information($"{nameof(SyncBackgroundTask)} is being started when the app is not running");
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //TODO: THIS BG TASK COULD RUN FOR MORE THAN 30 SEC...
            bool startedManually = taskInstance is null;
            if (!startedManually)
                taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);

            _deferral = taskInstance?.GetDeferral();
            _logger.Information($"Starting the {nameof(SyncBackgroundTask)} {(startedManually ? "manually" : "automatically" )}");

            if (_isAppRunning)
            {
                _messenger.Send(false, $"{MessageType.OPEN_PANE}");
                _messenger.Send(
                    new Tuple<bool, string>(
                        true, 
                        $"Performing a {(startedManually ? "manual" : "automatic" )} full sync, please wait..."),
                    $"{MessageType.SHOW_MAIN_PROGRESS_BAR}");
            }

            var syncResults = new List<EmptyResponse>
            {
                await _syncService.SyncDownTaskListsAsync(true),
                await _syncService.SyncDownTasksAsync(true),
                await _syncService.SyncUpTaskListsAsync(true),
                await _syncService.SyncUpTasksAsync(true)
            };

            string message = syncResults.Any(r => !r.Succeed) ?
                string.Join(",\n", syncResults.Where(r => !r.Succeed).Select(r => r.Message).Distinct()) :
                $"A {(startedManually ? "manual" : "automatic" )} full sync was successfully performed.";

            if (string.IsNullOrEmpty(message))
                message = "An unknown error occurred while trying to perform the sync operation.";

            _logger.Information($"{nameof(SyncBackgroundTask)} results = {message}");

            //TODO: MAYBE ADD A SETTING TO SHOW OR NOT SHOW THE NOTIFICATION

            if (!_isAppRunning && !startedManually && _appSettings.ShowToastNotificationAfterFullSync)
            {
                var content = GenerateToastContent(message);
                var toastNotification = new ToastNotification(content.GetXml());
                ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
            }
            else
            {
                _messenger.Send(message, $"{MessageType.SHOW_IN_APP_NOTIFICATION}");
                _messenger.Send(new Tuple<bool, string>(false, null), $"{MessageType.SHOW_MAIN_PROGRESS_BAR}");
                _messenger.Send(true, $"{MessageType.ON_FULL_SYNC}");
            }

            _logger.Information($"{nameof(SyncBackgroundTask)} completed successfully");
            _deferral?.Complete();
        }

        private ToastContent GenerateToastContent(string results)
        {
            return new ToastContent()
            {
                //Launch = "action=viewEvent&eventId=1983",
                Scenario = ToastScenario.Default,

                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = "Sync results"
                            },
                            new AdaptiveText()
                            {
                                Text = results
                            }
                        }
                    }
                },

                Actions = new ToastActionsCustom()
                {
                    Buttons =
                    {
                        new ToastButtonDismiss()
                    }
                }
            };
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _logger.Warning($"{sender.Task.Name} cancel requested... Cancel reason = {reason.ToString()}");
        }
    }
}
