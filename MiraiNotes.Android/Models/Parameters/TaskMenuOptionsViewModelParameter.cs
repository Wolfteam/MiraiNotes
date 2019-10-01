using MiraiNotes.Android.ViewModels;

namespace MiraiNotes.Android.Models.Parameters
{
    public class TaskMenuOptionsViewModelParameter
    {
        public TaskListItemViewModel TaskList { get; }
        public TaskItemViewModel Task { get; }

        private TaskMenuOptionsViewModelParameter(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            TaskList = taskList;
            Task = task;
        }

        public static TaskMenuOptionsViewModelParameter Instance(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            return new TaskMenuOptionsViewModelParameter(taskList, task);
        }
    }
}