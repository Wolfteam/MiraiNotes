using MiraiNotes.Android.ViewModels;
using System.Collections.Generic;

namespace MiraiNotes.Android.Models.Parameters
{
    public class MoveTaskDialogViewModelParameter : BaseSelectionParameter
    {
        public TaskListItemViewModel CurrentTaskList { get; set; }
        public TaskListItemViewModel NewTaskList { get; set; }

        private MoveTaskDialogViewModelParameter(
            TaskListItemViewModel currentTaskList,
            TaskListItemViewModel newTaskList,
            TaskItemViewModel task)
            : base(task)
        {
            CurrentTaskList = currentTaskList;
            NewTaskList = newTaskList;
        }

        private MoveTaskDialogViewModelParameter(
            TaskListItemViewModel currentTaskList,
            TaskListItemViewModel newTaskList,
            List<TaskItemViewModel> tasks)
            : base(tasks)
        {
            CurrentTaskList = currentTaskList;
            NewTaskList = newTaskList;
        }

        public static MoveTaskDialogViewModelParameter Instance(
            TaskListItemViewModel currentTaskList,
            TaskListItemViewModel newTaskList,
            TaskItemViewModel task)
            => new MoveTaskDialogViewModelParameter(currentTaskList, newTaskList, task);

        public static MoveTaskDialogViewModelParameter Instance(
            TaskListItemViewModel currentTaskList,
            TaskListItemViewModel newTaskList,
            List<TaskItemViewModel> tasks)
            => new MoveTaskDialogViewModelParameter(currentTaskList, newTaskList, tasks);
    }
}