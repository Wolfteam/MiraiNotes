using Android.OS;
using Android.Views;
using Google.Android.Material.BottomSheet;
using MiraiNotes.Android.Common.Utils;
using MvvmCross.Droid.Support.Design;
using MvvmCross.Droid.Support.Design.Extensions;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.ViewModels;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    public abstract class BaseBottomSheetDialogFragment<TViewModel> : MvxBottomSheetDialogFragment<TViewModel>
        where TViewModel : class, IMvxViewModel
    {
        private BottomSheetBehavior _bottomSheetBehavior;
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
        public abstract int LayoutId { get; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var ignore = base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(LayoutId, null);

            return view;
        }

        public override void OnStart()
        {
            base.OnStart();
            var parent = (View)View.Parent;
            _bottomSheetBehavior = BottomSheetBehavior.From(parent);
            _bottomSheetBehavior.PeekHeight = (int)AndroidUtils.ToPixel(512, Activity);
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