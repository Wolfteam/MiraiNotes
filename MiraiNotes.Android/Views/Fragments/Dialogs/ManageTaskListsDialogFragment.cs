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
    public class ManageTaskListsDialogFragment : MvxBottomSheetDialogFragment<ManageTaskListsDialogViewModel>
    {
        public int LayoutId
            => Resource.Layout.ManageTaskListsDialog;

        private IMvxInteraction<bool> _hideDialogRequest;
        public IMvxInteraction<bool> HideDialogRequest
        {
            get => _hideDialogRequest;
            set
            {
                if (_hideDialogRequest != null)
                    _hideDialogRequest.Requested -= (sender, args)
                        => HideDialog(args.Value);

                _hideDialogRequest = value;
                _hideDialogRequest.Requested += (sender, args)
                    => HideDialog(args.Value);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignore = base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(LayoutId, null);

            var set = this.CreateBindingSet<ManageTaskListsDialogFragment, ManageTaskListsDialogViewModel>();
            set.Bind(this).For(v => v.HideDialogRequest).To(vm => vm.HideDialog);
            set.Apply();

            return view;
        }

        public void HideDialog(bool hide)
        {
            var parent = View.Parent as View;
            if (hide)
                parent.HideWithTranslateAnimation();
            else
                parent.ShowWithTranslateAnimation();
        }
    }
}