using MiraiNotes.Android.ViewModels;
using System;

namespace MiraiNotes.Android.Models.Parameters
{
    public class TaskReminderDialogViewModelParameter
    {
        public TaskListItemViewModel TaskList { get; }
        public TaskItemViewModel Task { get; }
        private TaskReminderDialogViewModelParameter(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            TaskList = taskList ?? throw new ArgumentNullException(nameof(taskList));
            Task = task ?? throw new ArgumentNullException(nameof(task));
        }

        public static TaskReminderDialogViewModelParameter Instance(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            return new TaskReminderDialogViewModelParameter(taskList, task);
        }
    }
}