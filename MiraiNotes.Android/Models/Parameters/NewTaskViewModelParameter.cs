using MiraiNotes.Android.ViewModels;
using System;

namespace MiraiNotes.Android.Models.Parameters
{
    public class NewTaskViewModelParameter
    {
        public TaskListItemViewModel TaskList { get; }
        public string TaskId { get; }

        private NewTaskViewModelParameter(TaskListItemViewModel taskList, string taskId)
        {
            TaskList = taskList ?? throw new ArgumentNullException(nameof(taskList));
            TaskId = taskId ?? throw new ArgumentNullException(nameof(taskId));
        }

        public static NewTaskViewModelParameter Instance(TaskListItemViewModel taskList, string taskId)
        {
            return new NewTaskViewModelParameter(taskList, taskId);
        }
    }
}