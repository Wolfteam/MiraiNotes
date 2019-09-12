using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Platforms.Android.Presenters.Attributes;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    [MvxDialogFragmentPresentation(ActivityHostViewModelType = typeof(MainViewModel), Tag = nameof(TaskListDialogFragment))]
    public class TaskListDialogFragment : BaseDialogFragment<TaskListDialogViewModel>
    {
        public override int LayoutId 
            => Resource.Layout.TaskListDialog;
    }
}