using Microsoft.Toolkit.Uwp.Notifications;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using System.Linq;
using Windows.UI.Notifications;

namespace MiraiNotes.UWP.Services
{
    public class NotificationService : INotificationService
    {
        private const string TaskReminderToastTag = "TaskReminderToastTag";

        public void ShowNotification(TaskNotification notification)
        {
            var content = GenerateSimpleToastContent(
                notification.Title,
                notification.Content,
                notification.UwpSettings?.ShowDismissButton ?? true,
                notification.UwpSettings?.IsAudioSilent ?? false);
            var toastNotifcation = new ToastNotification(content.GetXml());
            if (!string.IsNullOrEmpty(notification.UwpSettings?.Tag))
                toastNotifcation.Tag = notification.UwpSettings.Tag;
            ShowToastNotification(toastNotifcation);
        }

        public void ScheduleNotification(TaskReminderNotification notification)
        {
            var content = GenerateTaskReminderToastContent(
               notification.TaskListId,
               notification.TaskId,
               notification.TaskListTitle,
               notification.TaskTitle,
               notification.TaskBody);
            var toastNotification = new ScheduledToastNotification(content.GetXml(), notification.DeliveryOn)
            {
                Id = $"{notification.Id}",
                Tag = TaskReminderToastTag
            };
            ScheduleToastNotification(toastNotification);
        }

        public void RemoveScheduledNotification(int id)
        {
            var scheduledToast = ToastNotificationManager
                .CreateToastNotifier()
                .GetScheduledToastNotifications()
                .FirstOrDefault(st => st.Id == $"{id}");

            if (scheduledToast != null)
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
            int taskListID,
            int taskID,
            string taskListTitle,
            string taskTitle,
            string taskBody)
        {
            return new ToastContent()
            {
                //maybe this could be the date that this task was set to be reminded
                //DisplayTimestamp = DateTimeOffset.Now,
                Launch = $"action={(int)NotificationActionType.OPEN_TASK}&taskListID={taskListID}&taskID={taskID}",
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
                            $"action={(int)NotificationActionType.MARK_AS_COMPLETED}&taskID={taskID}")
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