using MiraiNotes.Android.ViewModels;
using System.Collections.Generic;

namespace MiraiNotes.Android.Models.Parameters
{
    public class ChangeTaskStatusDialogViewModelParameter : BaseSelectionParameter
    {
        private ChangeTaskStatusDialogViewModelParameter(TaskItemViewModel task)
            : base(task)
        {
        }

        private ChangeTaskStatusDialogViewModelParameter(List<TaskItemViewModel> tasks)
            : base(tasks)
        {
        }

        public static ChangeTaskStatusDialogViewModelParameter ChangeTaskStatus(TaskItemViewModel task)
            => new ChangeTaskStatusDialogViewModelParameter(task);

        public static ChangeTaskStatusDialogViewModelParameter ChangeTaskStatus(List<TaskItemViewModel> tasks)
            => new ChangeTaskStatusDialogViewModelParameter(tasks);
    }
}