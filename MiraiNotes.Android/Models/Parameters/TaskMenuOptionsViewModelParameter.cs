using MiraiNotes.Android.ViewModels;
using System;

namespace MiraiNotes.Android.Models.Parameters
{
    public class TaskMenuOptionsViewModelParameter
    {
        public TaskListItemViewModel TaskList { get; }
        public TaskItemViewModel Task { get; }

        private TaskMenuOptionsViewModelParameter(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            TaskList = taskList ?? throw new ArgumentNullException(nameof(taskList));
            Task = task ?? throw new ArgumentNullException(nameof(task));
        }

        public static TaskMenuOptionsViewModelParameter Instance(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            return new TaskMenuOptionsViewModelParameter(taskList, task);
        }
    }
}