using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.Views.Fragments;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.ViewModels;

namespace MiraiNotes.Android.Views.Activities
{
    [MvxActivityPresentation]
    [Activity(
        Label = "@string/app_name",
        LaunchMode = LaunchMode.SingleTask
    )]
    public class MainActivity : BaseActivity<MainViewModel>
    {
        private IMvxInteraction<bool> _showDrawerRequest;
        private IMvxInteraction<AppThemeChangedMsg> _changeThemeRequest;
        private IMvxInteraction _hideKeyboardRequest;

        public IMvxInteraction<bool> ShowDrawerRequest
        {
            get => _showDrawerRequest;
            set
            {
                if (_showDrawerRequest != null)
                    _showDrawerRequest.Requested -= (sender, args)
                        => ShowDrawer(args.Value);

                _showDrawerRequest = value;
                _showDrawerRequest.Requested += (sender, args)
                        => ShowDrawer(args.Value);
            }
        }

        public IMvxInteraction<AppThemeChangedMsg> ChangeThemeRequest
        {
            get => _changeThemeRequest;
            set
            {
                if (_changeThemeRequest != null)
                    _changeThemeRequest.Requested -= (sender, args)
                        => SetAppTheme(args.Value.AppTheme, args.Value.AccentColor, args.Value.RestartActivity);

                _changeThemeRequest = value;
                _changeThemeRequest.Requested += (sender, args)
                        => SetAppTheme(args.Value.AppTheme, args.Value.AccentColor, args.Value.RestartActivity);
            }
        }

        public IMvxInteraction HideKeyboardRequest
        {
            get => _hideKeyboardRequest;
            set
            {
                if (_hideKeyboardRequest != null)
                    _hideKeyboardRequest.Requested -= (sender, args)
                        => HideSoftKeyboard();

                _hideKeyboardRequest = value;
                _hideKeyboardRequest.Requested += (sender, args)
                        => HideSoftKeyboard();
            }
        }


        public DrawerLayout DrawerLayout { get; private set; }
        public override int LayoutId
            => Resource.Layout.Main;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            DrawerLayout = FindViewById<DrawerLayout>(Resource.Id.AppDrawerLayout);

            var set = this.CreateBindingSet<MainActivity, MainViewModel>();
            set.Bind(this).For(v => v.ShowDrawerRequest).To(vm => vm.ShowDrawer).OneWay();
            set.Bind(this).For(v => v.ChangeThemeRequest).To(vm => vm.AppThemeChanged).OneWay();
            set.Bind(this).For(v => v.HideKeyboardRequest).To(vm => vm.HideKeyboard).OneWay();
            set.Apply();

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
            {
                DrawerLayout.CloseDrawers();
            }
            else if (SupportFragmentManager.FindFragmentById(Resource.Id.ContentFrame) is TasksFragment tasksFragment &&
                tasksFragment.IsFabOpen)
            {
                tasksFragment.CloseFabMenu();
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public void ShowDrawer(bool show)
        {
            if (show)
                DrawerLayout.OpenDrawer((int)GravityFlags.Start);
            else
                DrawerLayout.CloseDrawers();
        }
    }
}