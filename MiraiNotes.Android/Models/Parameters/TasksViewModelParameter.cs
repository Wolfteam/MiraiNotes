using MiraiNotes.Android.ViewModels;

namespace MiraiNotes.Android.Models.Parameters
{
    public class TasksViewModelParameter
    {
        public NotificationAction NotificationAction { get; }
        public TaskListItemViewModel TaskList { get; }

        private TasksViewModelParameter(NotificationAction notificationAction, TaskListItemViewModel taskList)
        {
            NotificationAction = notificationAction;
            TaskList = taskList;
        }

        public static TasksViewModelParameter Instance(NotificationAction notificationAction, TaskListItemViewModel taskList)
        {
            return new TasksViewModelParameter(notificationAction, taskList);
        }
    }
}