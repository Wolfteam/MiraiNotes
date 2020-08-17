using Android.OS;
using Android.Views;
using Google.Android.Material.BottomSheet;
using MiraiNotes.Android.Common.Utils;
using MvvmCross.Base;
using MvvmCross.DroidX.Material;
using MvvmCross.DroidX.Material.Extensions;
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
                    _hideDialogRequest.Requested -= HideDialogHandler;

                _hideDialogRequest = value;
                _hideDialogRequest.Requested += HideDialogHandler;
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

        private void HideDialogHandler(object sender  , MvxValueEventArgs<bool> args)
        {
            HideDialog(args.Value);
        }
    }
}