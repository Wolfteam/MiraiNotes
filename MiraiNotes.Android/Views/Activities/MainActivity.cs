using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Java.Util;
using MiraiNotes.Android.ViewModels;
using MvvmCross;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Localization;
using MvvmCross.Platforms.Android.Views;
using MvvmCross.Plugin.Messenger;
using System;
using Android.Graphics;
using Android.Support.V4.Widget;
using Android.Widget;
using Com.Mikepenz.Materialdrawer;
using Com.Mikepenz.Materialdrawer.Model;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using ActionBarDrawerToggle = Android.Support.V7.App.ActionBarDrawerToggle;

namespace MiraiNotes.Android
{
    [Activity(Label = "@string/app_name")]
    public class MainActivity : MvxAppCompatActivity<MainViewModel>
    {
        private MvxSubscriptionToken _token;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetTheme(MainViewModel.IsDarkTheme);
            _token = ViewModel.Messenger.Subscribe<ChangeThemeMsg>(msg =>
            {
                var intent = new Intent(this, typeof(MainActivity));
                intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                Finish();
                StartActivity(intent);
            });
            SetContentView(Resource.Layout.Main);

            var navView = FindViewById<NavigationView>(Resource.Id.AppNavView);

            var menu = navView.Menu;

            for (int i = 0; i < 40; i++)
            {
                menu.Add($"CustomMenu {i}").SetIcon(global::Android.Resource.Drawable.IcMenuGallery);
            }

            var subMenu = menu.AddSubMenu("Settings");
            for (int i = 1; i <= 2; i++)
            {
                subMenu.Add("SubMenu Item " + i);
            }

            for (int i = 0, count = navView.ChildCount; i < count; i++)
            {
                var child = navView.GetChildAt(i);
                if (child != null && child is ListView lv)
                {
                    ListView menuView = lv;
                    HeaderViewListAdapter adapter = (HeaderViewListAdapter) menuView.Adapter;
                    BaseAdapter wrapped = (BaseAdapter) adapter.WrappedAdapter;
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

//            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
            View view = (View) sender;
            Snackbar.Make(view, "Replace with your own action", Snackbar.LengthLong)
                .SetAction("Action", (View.IOnClickListener) null).Show();
        }

        private void SetTheme(bool dark)
        {
            if (dark)
                SetTheme(Resource.Style.Theme_MiraiNotes_Dark);
            else
                SetTheme(Resource.Style.Theme_MiraiNotes_Light);
        }

        private void InitDrawer()
        {
            var item1 = new PrimaryDrawerItem();
            item1.WithName("Item 1");
            item1.WithIdentifier(1);

            var item2 = new PrimaryDrawerItem();
            item1.WithName("Item 2");
            item1.WithIdentifier(2);

            var drawer = new DrawerBuilder()
                .WithActivity(this)
                .AddDrawerItems(
                    item1,
                    new DividerDrawerItem(),
                    item2,
                    new DividerDrawerItem())
                .Build();
        }
    }
}