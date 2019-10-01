using Android.OS;
using Android.Views;
using MiraiNotes.Android.Views.Activities;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.ViewModels;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    public abstract class BaseDialogFragment<TViewModel> : MvxDialogFragment<TViewModel> 
        where TViewModel : class, IMvxViewModel
    {
        public MvxAppCompatActivity ParentActivity
            => (MvxAppCompatActivity)Activity;

        public MainActivity MainActivity
            => (MainActivity)Activity;

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
            Dialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
        }
    }
}