using Android.OS;
using Android.Views;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Droid.Support.Design;
using MvvmCross.Platforms.Android.Binding.BindingContext;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    public class TasksMenuOptionsDialogFragment : MvxBottomSheetDialogFragment<TaskMenuOptionsViewModel>
    {
        public int LayoutId
            => Resource.Layout.TaskMenuOptionsDialog;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignore = base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(LayoutId, null);

            return view;
        }
    }
}