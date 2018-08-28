﻿using Microsoft.Toolkit.Uwp.Notifications;
using MiraiNotes.Shared.Models;
using MiraiNotes.UWP.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;

namespace MiraiNotes.UWP.BackgroundTasks
{
    public sealed class SyncBackgroundTask : IBackgroundTask
    {
        private readonly ISyncService _syncService;
        public SyncBackgroundTask(ISyncService syncService)
        {
            _syncService = syncService;
        }

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var syncResults = new List<EmptyResponse>
            {
                await _syncService.SyncDownTaskListsAsync(true),
                await _syncService.SyncDownTasksAsync(true),
                await _syncService.SyncUpTaskListsAsync(true),
                await _syncService.SyncUpTasksAsync(true)
            };

            string message = syncResults.Any(r => !r.Succeed) ?
                string.Join(",\n", syncResults.Where(r => !r.Succeed).Select(r => r.Message).Distinct()) :
                "A full sync was successfully performed.";

            if (string.IsNullOrEmpty(message))
                message = "An unknown error occurred while trying to perform the sync operation.";

            //TODO: MAYBE ADD A SETTING TO SHOW OR NOT SHOW THE NOTIFICATION
            // Generate the toast notification content and pop the toast
            var content = GenerateToastContent(message);
            var toastNotification = new ToastNotification(content.GetXml());
            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }

        public ToastContent GenerateToastContent(string results)
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
    }
}
