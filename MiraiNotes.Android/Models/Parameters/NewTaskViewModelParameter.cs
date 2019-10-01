using MiraiNotes.Android.ViewModels;

namespace MiraiNotes.Android.Models.Parameters
{
    public class NewTaskViewModelParameter
    {
        public TaskListItemViewModel TaskList { get; }
        public string TaskId { get; }

        private NewTaskViewModelParameter(TaskListItemViewModel taskList, string taskId)
        {
            TaskList = taskList;
            TaskId = taskId;
        }

        public static NewTaskViewModelParameter Instance(TaskListItemViewModel taskList, string taskId)
        {
            return new NewTaskViewModelParameter(taskList, taskId);
        }
    }
}