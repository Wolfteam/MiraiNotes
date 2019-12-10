using MiraiNotes.Android.ViewModels;
using System;

namespace MiraiNotes.Android.Models.Parameters
{
    public class TaskListsDialogViewModelParameter
    {
        public bool Move { get; }
        public bool Select { get; }
        public TaskListItemViewModel TaskList { get; }
        public TaskItemViewModel Task { get; }

        private TaskListsDialogViewModelParameter(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            TaskList = taskList ?? throw new ArgumentNullException(nameof(taskList));
            Task = task ?? throw new ArgumentNullException(nameof(task));
            Move = true;
        }

        private TaskListsDialogViewModelParameter(TaskListItemViewModel taskList)
        {
            TaskList = taskList ?? throw new ArgumentNullException(nameof(taskList));
            Select = true;
        }

        public static TaskListsDialogViewModelParameter MoveTo(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            return new TaskListsDialogViewModelParameter(taskList, task);
        }

        public static TaskListsDialogViewModelParameter SelectTaskList(TaskListItemViewModel currentTaskList)
        {
            return new TaskListsDialogViewModelParameter(currentTaskList);
        }
    }
}