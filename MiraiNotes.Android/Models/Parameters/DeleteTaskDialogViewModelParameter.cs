using MiraiNotes.Android.ViewModels;
using System.Collections.Generic;

namespace MiraiNotes.Android.Models.Parameters
{
    public class DeleteTaskDialogViewModelParameter : BaseSelectionParameter
    {
        private DeleteTaskDialogViewModelParameter(TaskItemViewModel task)
            : base(task)
        {
        }

        private DeleteTaskDialogViewModelParameter(List<TaskItemViewModel> tasks)
            : base(tasks)
        {
        }

        public static DeleteTaskDialogViewModelParameter Delete(TaskItemViewModel task)
            => new DeleteTaskDialogViewModelParameter(task);

        public static DeleteTaskDialogViewModelParameter Delete(List<TaskItemViewModel> tasks)
            => new DeleteTaskDialogViewModelParameter(tasks);
    }
}