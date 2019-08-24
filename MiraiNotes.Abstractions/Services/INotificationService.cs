using MiraiNotes.Core.Models;

namespace MiraiNotes.Abstractions.Services
{
    public interface INotificationService
    {
        /// <summary>
        /// Shows a simple toast notification with the title and the body specified
        /// </summary>
        /// <param name="notification">The notification options</param>
        void ShowNotification(TaskNotification notification);

        /// <summary>
        /// Schedules a toast notification that serves as a reminder for a task
        /// </summary>
        /// <param name="notification">The notification options</param>
        void ScheduleNotification(TaskReminderNotification notification);

        /// <summary>
        /// Removes a scheduled toast that matches the <paramref name="id"/>
        /// </summary>
        /// <param name="id">Id of the scheduled toast to remove</param>
        void RemoveScheduledNotification(int id);
    }
}
