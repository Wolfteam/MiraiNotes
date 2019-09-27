using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Platforms.Android.Presenters.Attributes;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    [MvxDialogFragmentPresentation(ActivityHostViewModelType = typeof(MainViewModel), Tag = nameof(DeleteTaskFragment), Cancelable = true)]
    public class DeleteTaskFragment : BaseDialogFragment<DeleteTaskDialogViewModel>
    {
        public override int LayoutId
            => Resource.Layout.ConfirmationDialog;
    }
}