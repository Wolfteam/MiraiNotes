using MiraiNotes.Android.ViewModels;
using System;

namespace MiraiNotes.Android.Models.Parameters
{
    public class TasksViewModelParameter
    {
        public NotificationAction NotificationAction { get; }
        public TaskListItemViewModel TaskList { get; }

        private TasksViewModelParameter(NotificationAction notificationAction, TaskListItemViewModel taskList)
        {
            NotificationAction = notificationAction;
            TaskList = taskList ?? throw new ArgumentNullException(nameof(taskList));
        }

        public static TasksViewModelParameter Instance(NotificationAction notificationAction, TaskListItemViewModel taskList)
        {
            return new TasksViewModelParameter(notificationAction, taskList);
        }
    }
}