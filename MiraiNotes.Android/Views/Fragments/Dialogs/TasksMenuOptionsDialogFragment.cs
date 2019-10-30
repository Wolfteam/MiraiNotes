using MiraiNotes.Android.ViewModels.Dialogs;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    public class TasksMenuOptionsDialogFragment : BaseBottomSheetDialogFragment<TaskMenuOptionsViewModel>
    {
        public override int LayoutId
            => Resource.Layout.TaskMenuOptionsDialog;
    }
}