using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Platforms.Android.Presenters.Attributes;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    [MvxDialogFragmentPresentation(ActivityHostViewModelType = typeof(MainViewModel), Tag = nameof(MoveTaskDialogFragment), Cancelable = true)]
    public class MoveTaskDialogFragment : BaseDialogFragment<MoveTaskDialogViewModel>
    {
        public override int LayoutId
            => Resource.Layout.ConfirmationDialog;
    }
}