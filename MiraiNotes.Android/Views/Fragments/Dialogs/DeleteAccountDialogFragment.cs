using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Platforms.Android.Presenters.Attributes;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    [MvxDialogFragmentPresentation(ActivityHostViewModelType = typeof(MainViewModel), Tag = nameof(DeleteAccountDialogFragment), Cancelable = true)]
    public class DeleteAccountDialogFragment : BaseDialogFragment<DeleteAccountDialogViewModel>
    {
        public override int LayoutId
            => Resource.Layout.ConfirmationDialog;
    }
}