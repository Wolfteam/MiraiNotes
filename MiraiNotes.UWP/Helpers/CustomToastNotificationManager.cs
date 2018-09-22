using Microsoft.Toolkit.Uwp.Notifications;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System;
using System.Linq;
using Windows.UI.Notifications;

namespace MiraiNotes.UWP.Helpers
{
    public class CustomToastNotificationManager : ICustomToastNotificationManager
    {
        public void ShowSimpleToastNotification(string title, string body, bool showDismissButton = true)
        {
            var content = GenerateSimpleToastContent(title, body, showDismissButton);
            var toastNotifcation = new ToastNotification(content.GetXml());
            ShowToastNotification(toastNotifcation);
        }

        public void ShowSimpleToastNotification(
            string title,
            string body,
            string tag,
            bool showDismissButton = true,
            bool isAudioSilent = false)
        {
            var content = GenerateSimpleToastContent(title, body, showDismissButton, isAudioSilent);
            var toastNotifcation = new ToastNotification(content.GetXml());
            if (!string.IsNullOrEmpty(tag))
                toastNotifcation.Tag = tag;
            ShowToastNotification(toastNotifcation);
        }

        public void ScheduleTaskReminderToastNotification(
            string toastID,
            string taskListID,
            string taskID,
            string taskListTitle,
            string taskTitle,
            string taskBody,
            DateTimeOffset deliveryTime)
        {
            var content = GenerateTaskReminderToastContent(taskListID, taskID, taskListTitle, taskTitle, taskBody);
            var toastNotification = new ScheduledToastNotification(content.GetXml(), deliveryTime)
            {
                Id = toastID,
                Tag = "TaskReminderToastTag"
            };
            ScheduleToastNotification(toastNotification);
        }

        public void RemoveScheduledToast(string id)
        {
            var scheduledToast = ToastNotificationManager
                .CreateToastNotifier()
                .GetScheduledToastNotifications()
                .FirstOrDefault(st => st.Id == id);

            if (scheduledToast is null == false)
                ToastNotificationManager.CreateToastNotifier().RemoveFromSchedule(scheduledToast);
        }

        private void ShowToastNotification(ToastNotification toast)
            => ToastNotificationManager.CreateToastNotifier().Show(toast);

        private void ScheduleToastNotification(ScheduledToastNotification scheduledToast)
            => ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledToast);

        private ToastContent GenerateSimpleToastContent(string title, string content, bool showDismissButton = true, bool isAudioSilent = false)
        {
            var audio = new ToastAudio
            {
                Silent = isAudioSilent
            };
            if (showDismissButton)
            {
                return new ToastContent()
                {
                    Audio = audio,
                    Scenario = ToastScenario.Default,
                    Visual = new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                        {
                            new AdaptiveText()
                            {
                                Text = title
                            },
                            new AdaptiveText()
                            {
                                Text = content
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
            else
            {
                return new ToastContent()
                {
                    Audio = audio,
                    Scenario = ToastScenario.Default,
                    Visual = new ToastVisual()
                    {
                        BindingGeneric = new ToastBindingGeneric()
                        {
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title
                                },
                                new AdaptiveText()
                                {
                                    Text = content
                                }
                            }
                        }
                    }
                };
            }
        }

        private ToastContent GenerateTaskReminderToastContent(
            string taskListID,
            string taskID,
            string taskListTitle,
            string taskTitle,
            string taskBody)
        {
            return new ToastContent()
            {
                //maybe this could be the date that this task was set to be reminded
                //DisplayTimestamp = DateTimeOffset.Now,
                Launch = $"action={(int)ToastNotificationActionType.OPEN_TASK}&taskListID={taskListID}&taskID={taskID}",
                Scenario = ToastScenario.Reminder,
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = taskListTitle
                            },
                            new AdaptiveText()
                            {
                                Text = taskTitle
                            },
                            new AdaptiveText()
                            {
                                Text = taskBody
                            }
                        }
                    }
                },
                Actions = new ToastActionsCustom()
                {
                    Inputs =
                    {
                        new ToastSelectionBox("snoozeTimeID")
                        {
                            Items =
                            {
                                new ToastSelectionBoxItem("10", "Remind me in 10 min"),
                                new ToastSelectionBoxItem("30", "Remind me in 30 min"),
                                new ToastSelectionBoxItem("60", "Remind me in 1hr"),
                            },
                            Title = "Snooze for:",
                            DefaultSelectionBoxItemId = "30"
                        }
                    },
                    Buttons =
                    {
                        new ToastButtonSnooze()
                        {
                            SelectionBoxId = "snoozeTimeID",
                            HintActionId = "SnoozeTaskReminder"
                        },
                        new ToastButton(
                            "Complete",
                            $"action={(int)ToastNotificationActionType.MARK_AS_COMPLETED}&taskID={taskID}")
                        {
                            ActivationType = ToastActivationType.Background,
                            ActivationOptions = new ToastActivationOptions()
                            {
                                AfterActivationBehavior = ToastAfterActivationBehavior.PendingUpdate
                            },
                            HintActionId = "TaskMarkedAsCompleted"
                        },
                        new ToastButtonDismiss()
                        {
                            HintActionId = "DissmissTaskReminder"
                        },
                    }
                }
            };
        }
    }
}