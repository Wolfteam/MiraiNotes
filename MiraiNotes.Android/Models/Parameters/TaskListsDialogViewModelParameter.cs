using MiraiNotes.Android.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraiNotes.Android.Models.Parameters
{
    public class TaskListsDialogViewModelParameter
    {
        public bool IsMultipleTasks
            => Tasks.Any();
        public bool Move { get; }
        public bool Select { get; }
        public TaskListItemViewModel TaskList { get; }
        public TaskItemViewModel Task { get; }
        public List<TaskItemViewModel> Tasks { get; }
            = new List<TaskItemViewModel>();

        private TaskListsDialogViewModelParameter(TaskListItemViewModel taskList, TaskItemViewModel task)
        {
            TaskList = taskList ?? throw new ArgumentNullException(nameof(taskList));
            Task = task ?? throw new ArgumentNullException(nameof(task));
            Move = true;
        }

        private TaskListsDialogViewModelParameter(TaskListItemViewModel taskList, List<TaskItemViewModel> tasks)
        {
            TaskList = taskList ?? throw new ArgumentNullException(nameof(taskList));
            Tasks = tasks;
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

        public static TaskListsDialogViewModelParameter MoveTo(TaskListItemViewModel taskList, List<TaskItemViewModel> tasks)
        {
            return new TaskListsDialogViewModelParameter(taskList, tasks);
        }

        public static TaskListsDialogViewModelParameter SelectTaskList(TaskListItemViewModel currentTaskList)
        {
            return new TaskListsDialogViewModelParameter(currentTaskList);
        }
    }
}