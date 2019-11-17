using MiraiNotes.Android.ViewModels;
using System;

namespace MiraiNotes.Android.Models.Parameters
{
    public class MoveToTaskListDialogViewModelParameter
    {
        public TaskListItemViewModel TaskList { get; }
        public TaskItemViewModel Task { get; }

        private MoveToTaskListDialogViewModelParameter(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            TaskList = taskList ?? throw new ArgumentNullException(nameof(taskList));
            Task = task ?? throw new ArgumentNullException(nameof(task));
        }


        public static MoveToTaskListDialogViewModelParameter Instance(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            return new MoveToTaskListDialogViewModelParameter(taskList, task);
        }
    }
}