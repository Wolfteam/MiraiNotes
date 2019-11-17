using MiraiNotes.Android.ViewModels;
using System;

namespace MiraiNotes.Android.Models.Parameters
{
    public class AddSubTaskDialogViewModelParameter
    {
        public bool Notify { get; }
        public string TaskListId { get; }
        public TaskItemViewModel Task { get; }
        private AddSubTaskDialogViewModelParameter(string taskListId, TaskItemViewModel task, bool notify)
        {
            TaskListId = taskListId ?? throw new ArgumentNullException(nameof(taskListId));
            Task = task ?? throw new ArgumentNullException(nameof(task));
            Notify = notify;
        }

        public static AddSubTaskDialogViewModelParameter Instance(string taskListId, TaskItemViewModel task, bool notify = false)
        {
            return new AddSubTaskDialogViewModelParameter(taskListId, task, notify);
        }
    }
}