using MiraiNotes.Android.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace MiraiNotes.Android.Models.Parameters
{
    public class DeleteTaskDialogViewModelParameter
    {
        public bool IsMultipleDeletes
            => Tasks.Any();
        public TaskItemViewModel Task { get; set; }
        public List<TaskItemViewModel> Tasks { get; } 
            = new List<TaskItemViewModel>();

        private DeleteTaskDialogViewModelParameter(TaskItemViewModel task)
        {
            Task = task;
        }

        private DeleteTaskDialogViewModelParameter(List<TaskItemViewModel> tasks)
        {
            Tasks = tasks;
        }

        public static DeleteTaskDialogViewModelParameter Delete(TaskItemViewModel task)
            => new DeleteTaskDialogViewModelParameter(task);

        public static DeleteTaskDialogViewModelParameter Delete(List<TaskItemViewModel> tasks)
            => new DeleteTaskDialogViewModelParameter(tasks);
    }
}