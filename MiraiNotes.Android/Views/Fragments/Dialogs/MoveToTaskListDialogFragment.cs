using Android.OS;
using Android.Views;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Droid.Support.Design;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    public class MoveToTaskListDialogFragment : MvxBottomSheetDialogFragment<MoveToTaskListDialogViewModel>
    {
        public int LayoutId
            => Resource.Layout.MoveToTaskListDialog;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignore = base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(LayoutId, null);

            return view;
        }
    }
}