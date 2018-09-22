using System;

namespace MiraiNotes.UWP.Interfaces
{
    public interface ICustomToastNotificationManager
    {
        /// <summary>
        /// Shows a simple toast notification with the title and the body specified
        /// </summary>
        /// <param name="title">The title of the toast</param>
        /// <param name="body">The body of the toast</param>
        /// <param name="showDismissButton">Indicates if the toast shows a dismiss button</param>
        void ShowSimpleToastNotification(string title, string body, bool showDismissButton = true);

        /// <summary>
        /// Shows a simple toast notification with the title and the body specified
        /// </summary>
        /// <param name="title">The title of the toast</param>
        /// <param name="body">The body of the toast</param>
        /// <param name="showDismissButton">Indicates if the toast shows a dismiss button</param>
        /// <param name="tag">A tag for this toast. 
        /// If you are already showing a toast with this tag, it will be replaced by the new one</param>
        /// <param name="isAudioSilent">If true, the toast will not generate a sound</param>
        void ShowSimpleToastNotification(
            string title, 
            string body,
            string tag, 
            bool showDismissButton = true, 
            bool isAudioSilent = false);

        /// <summary>
        /// Schedules a toast notification that serves as a reminder for a task
        /// </summary>
        /// <param name="toastID">An identifier for the scheduled toast</param>
        /// <param name="taskListID">Task list id, used when the user clicks on the toast</param>
        /// <param name="taskID">Task id, used when the user clicks on the toast</param>
        /// <param name="taskListTitle">Task list title</param>
        /// <param name="taskTitle">Task title</param>
        /// <param name="taskBody">Task body</param>
        /// <param name="deliveryTime">The DateTimeOffset that this toast will be delivered</param>
        void ScheduleTaskReminderToastNotification(
            string toastID,
            string taskListID,
            string taskID,
            string taskListTitle,
            string taskTitle,
            string taskBody,
            DateTimeOffset deliveryTime);

        /// <summary>
        /// Removes a scheduled toast that matches the <paramref name="id"/>
        /// </summary>
        /// <param name="id">Id of the scheduled toast to remove</param>
        void RemoveScheduledToast(string id);
    }
}
