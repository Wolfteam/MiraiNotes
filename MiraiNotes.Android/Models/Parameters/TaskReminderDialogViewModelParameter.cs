using MiraiNotes.Android.ViewModels;

namespace MiraiNotes.Android.Models.Parameters
{
    public class TaskReminderDialogViewModelParameter
    {
        public TaskListItemViewModel TaskList { get; }
        public TaskItemViewModel Task { get; }
        private TaskReminderDialogViewModelParameter(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            TaskList = taskList;
            Task = task;
        }

        public static TaskReminderDialogViewModelParameter Instance(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            return new TaskReminderDialogViewModelParameter(taskList, task);
        }
    }
}