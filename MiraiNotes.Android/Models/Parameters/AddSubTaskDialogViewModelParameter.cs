using MiraiNotes.Android.ViewModels;
using System;

namespace MiraiNotes.Android.Models.Parameters
{
    public class AddSubTaskDialogViewModelParameter
    {
        public string TaskListId { get; }
        public TaskItemViewModel Task { get; }
        private AddSubTaskDialogViewModelParameter(string taskListId, TaskItemViewModel task)
        {
            TaskListId = taskListId ?? throw new ArgumentNullException(nameof(taskListId));
            Task = task ?? throw new ArgumentNullException(nameof(task));
        }

        public static AddSubTaskDialogViewModelParameter Instance(string taskListId, TaskItemViewModel task)
        {
            return new AddSubTaskDialogViewModelParameter(taskListId, task);
        }
    }
}