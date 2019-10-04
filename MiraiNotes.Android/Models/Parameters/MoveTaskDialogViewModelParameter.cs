using MiraiNotes.Android.ViewModels;

namespace MiraiNotes.Android.Models.Parameters
{
    public class MoveTaskDialogViewModelParameter
    {
        public TaskListItemViewModel CurrentTaskList { get; set; }
        public TaskListItemViewModel NewTaskList { get; set; }
        public TaskItemViewModel Task { get; set; }

        private MoveTaskDialogViewModelParameter(TaskListItemViewModel currentTaskList, TaskListItemViewModel newTaskList, TaskItemViewModel task)
        {
            CurrentTaskList = currentTaskList;
            NewTaskList = newTaskList;
            Task = task;
        }

        public static MoveTaskDialogViewModelParameter Instance(TaskListItemViewModel currentTaskList, TaskListItemViewModel newTaskList, TaskItemViewModel task)
        {
            return new MoveTaskDialogViewModelParameter(currentTaskList, newTaskList, task);
        }
    }
}