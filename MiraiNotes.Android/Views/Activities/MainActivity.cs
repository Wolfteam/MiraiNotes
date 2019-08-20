using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Views.InputMethods;
using MiraiNotes.Android.ViewModels;
using MvvmCross.Droid.Support.V7.AppCompat;
using System;

namespace MiraiNotes.Android
{
    [Activity(Label = "@string/app_name", LaunchMode = LaunchMode.SingleTask)]
    public class MainActivity : MvxAppCompatActivity<MainViewModel>
    {
        public DrawerLayout DrawerLayout { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(MainViewModel.IsDarkTheme);
            var token = ViewModel.Messenger.Subscribe<ChangeThemeMsg>(msg =>
            {
                var intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                Finish();
                StartActivity(intent);
            });
            ViewModel.SubscriptionTokens.Add(token);
            SetContentView(Resource.Layout.Main);

            DrawerLayout = FindViewById<DrawerLayout>(Resource.Id.AppDrawerLayout);

            ViewModel.InitViewCommand.Execute();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            switch (id)
            {
                case Resource.Id.Accounts:
                    ViewModel.OnAccountsSelectedCommand.Execute();
                    break;
                case Resource.Id.Settings:
                    ViewModel.OnSettingsSelectedCommand.Execute();
                    break;
                case Resource.Id.Logout:
                    ViewModel.LogoutCommand.Execute(null);
                    break;
                default:
                    return false;
            }

            return true;
        }

        public override void OnBackPressed()
        {
            if (DrawerLayout != null && DrawerLayout.IsDrawerOpen(GravityCompat.Start))
                DrawerLayout.CloseDrawers();
            else
                base.OnBackPressed();
        }

        public void HideSoftKeyboard()
        {
            if (CurrentFocus == null)
                return;

            var inputMethodManager = (InputMethodManager)GetSystemService(InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);

            CurrentFocus.ClearFocus();
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View)sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (View.IOnClickListener)null).Show();
        }

        private void SetTheme(bool dark)
        {
            if (dark)
                SetTheme(Resource.Style.Theme_MiraiNotes_Dark);
            else
                SetTheme(Resource.Style.Theme_MiraiNotes_Light);
        }
        /*
        private void InitDrawer()
        {
            _navView = FindViewById<NavigationView>(Resource.Id.AppNavView);

            var menu = _navView.Menu;

            for (int i = 0; i < 40; i++)
            {
                menu.Add($"CustomMenu {i}").SetIcon(global::Android.Resource.Drawable.IcMenuGallery);
            }

            var subMenu = menu.AddSubMenu("Settings");
            for (int i = 1; i <= 2; i++)
            {
                subMenu.Add("SubMenu Item " + i);
            }

            for (int i = 0, count = _navView.ChildCount; i < count; i++)
            {
                var child = _navView.GetChildAt(i);
                if (child != null && child is ListView lv)
                {
                    ListView menuView = lv;
                    HeaderViewListAdapter adapter = (HeaderViewListAdapter)menuView.Adapter;
                    BaseAdapter wrapped = (BaseAdapter)adapter.WrappedAdapter;
                    wrapped.NotifyDataSetChanged();
                }
            }

            var toolbar = FindViewById<Toolbar>(Resource.Id.AppToolbar);
            //            toolbar.Title = "Task list 1";
            //            toolbar.ShowOverflowMenu();
            //            toolbar.ShowContextMenu();
            SetSupportActionBar(toolbar);

            var drawerLayout = FindViewById<DrawerLayout>(Resource.Id.AppDrawerLayout);
            var drawerToggle = new ActionBarDrawerToggle(this, drawerLayout, toolbar, Resource.String.Open,
                Resource.String.Close);

            // Set the drawer toggle as the DrawerListener 
            drawerLayout.AddDrawerListener(drawerToggle);
            SupportActionBar.SetDisplayHomeAsUpEnabled(false);
            SupportActionBar.SetHomeButtonEnabled(true);
            drawerToggle.SyncState();
        }
        */
    }
}