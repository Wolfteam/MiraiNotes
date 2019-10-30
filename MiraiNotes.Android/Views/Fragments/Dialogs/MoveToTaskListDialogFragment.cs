using Android.OS;
using Android.Views;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Binding.BindingContext;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    public class MoveToTaskListDialogFragment : BaseBottomSheetDialogFragment<MoveToTaskListDialogViewModel>
    {
        public override int LayoutId
            => Resource.Layout.MoveToTaskListDialog;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);

            var set = this.CreateBindingSet<MoveToTaskListDialogFragment, MoveToTaskListDialogViewModel>();
            set.Bind(this).For(v => v.HideDialogRequest).To(vm => vm.HideDialog);
            set.Apply();

            return view;
        }
    }
}