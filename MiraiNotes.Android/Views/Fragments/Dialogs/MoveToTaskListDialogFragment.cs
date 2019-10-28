using Android.OS;
using Android.Views;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Droid.Support.Design;
using MvvmCross.Droid.Support.Design.Extensions;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.ViewModels;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    public class MoveToTaskListDialogFragment : MvxBottomSheetDialogFragment<MoveToTaskListDialogViewModel>
    {
        public int LayoutId
            => Resource.Layout.MoveToTaskListDialog;

        private IMvxInteraction _hideDialogRequest;
        public IMvxInteraction HideDialogRequest
        {
            get => _hideDialogRequest;
            set
            {
                if (_hideDialogRequest != null)
                    _hideDialogRequest.Requested -= (sender, args)
                        => HideDialog();

                _hideDialogRequest = value;
                _hideDialogRequest.Requested += (sender, args)
                    => HideDialog();
            }
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignore = base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(LayoutId, null);
            var set = this.CreateBindingSet<MoveToTaskListDialogFragment, MoveToTaskListDialogViewModel>();
            set.Bind(this).For(v => v.HideDialogRequest).To(vm => vm.HideDialog);
            set.Apply();

            return view;
        }

        public void HideDialog()
        {
            var parent = View.Parent as View;
            parent.HideWithTranslateAnimation();
        }
    }
}