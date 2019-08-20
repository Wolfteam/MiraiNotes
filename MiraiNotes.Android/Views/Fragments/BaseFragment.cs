using Android.Content.Res;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.ViewModels;

namespace MiraiNotes.Android.Views.Fragments
{
    public abstract class BaseFragment<TViewModel> : MvxFragment<TViewModel> where TViewModel : class, IMvxViewModel
    {
        private MvxActionBarDrawerToggle _drawerToggle;
        private Toolbar _toolbar;

        public MvxAppCompatActivity ParentActivity
            => (MvxAppCompatActivity)Activity;

        public MainActivity MainActivity
            => (MainActivity)Activity;

        protected abstract int FragmentId { get; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(FragmentId, null);

            _toolbar = view.FindViewById<Toolbar>(Resource.Id.AppToolbar);
            _toolbar.ShowOverflowMenu();
            _toolbar.ShowContextMenu();
            ParentActivity.SetSupportActionBar(_toolbar);

            var drawerLayout = ((MainActivity)ParentActivity).DrawerLayout;
            _drawerToggle = new MvxActionBarDrawerToggle(
                Activity,
                drawerLayout,
                _toolbar,
                Resource.String.Open,
                Resource.String.Close);

            // Set the drawer toggle as the DrawerListener 
            drawerLayout.AddDrawerListener(_drawerToggle);
            ParentActivity.SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            ParentActivity.SupportActionBar.SetHomeButtonEnabled(true);

            _drawerToggle.DrawerOpened += (sender, args)
                => MainActivity?.HideSoftKeyboard();
            MainActivity.DrawerLayout.AddDrawerListener(_drawerToggle);

            _drawerToggle.SyncState();

            return view;
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            if (_toolbar != null)
                _drawerToggle.OnConfigurationChanged(newConfig);
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            if (_toolbar != null)
                _drawerToggle.SyncState();
        }
    }
}