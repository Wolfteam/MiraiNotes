using MiraiNotes.Android.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiraiNotes.Android.Models.Parameters
{
    public class BaseSelectionParameter
    {
        public bool IsMultipleTasks
            => Tasks.Any();
        public TaskItemViewModel Task { get; }
        public List<TaskItemViewModel> Tasks { get; }
            = new List<TaskItemViewModel>();

        protected BaseSelectionParameter(TaskItemViewModel task)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));
        }

        protected BaseSelectionParameter(List<TaskItemViewModel> tasks)
        {
            if (!tasks.Any())
                throw new Exception("You need to provide at least one task to be deleted");
            Tasks = tasks;
        }
    }
}