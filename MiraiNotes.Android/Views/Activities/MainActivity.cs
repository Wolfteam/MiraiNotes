using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using MiraiNotes.Android.Models;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.Views.Fragments;
using MiraiNotes.Core.Enums;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.ViewModels;
using Newtonsoft.Json;
using System;

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
        private IMvxInteraction _changeLanguageRequest;
        private IMvxInteraction<TaskSortType> _updateTaskSortOrderRequest;
        private bool _lockDrawerRequest;
        private TaskSortType _selectedTaskSortType;

        public const string InitParamsKey = "InitParams";


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
        public IMvxInteraction ChangeLanguageRequest
        {
            get => _changeLanguageRequest;
            set
            {
                if (_changeLanguageRequest != null)
                    _changeLanguageRequest.Requested -= (sender, args)
                        => RestartActivity();

                _changeLanguageRequest = value;
                _changeLanguageRequest.Requested += (sender, args)
                        => RestartActivity();
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
        public IMvxInteraction<TaskSortType> UpdateTaskSortOrderRequest
        {
            get => _updateTaskSortOrderRequest;
            set
            {
                if (_updateTaskSortOrderRequest != null)
                    _updateTaskSortOrderRequest.Requested -= (sender, args)
                        => SetSelectedTaskSortType(args.Value);

                _updateTaskSortOrderRequest = value;
                _updateTaskSortOrderRequest.Requested += (sender, args)
                        => SetSelectedTaskSortType(args.Value);
            }
        }

        public DrawerLayout DrawerLayout { get; private set; }
        public override int LayoutId
            => Resource.Layout.Main;
        public bool LockDrawerRequest
        {
            get => _lockDrawerRequest;
            set
            {
                _lockDrawerRequest = value;
                LockDrawer(_lockDrawerRequest);
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var extra = Intent?.GetStringExtra(InitParamsKey);
            if (!string.IsNullOrEmpty(extra))
            {
                ViewModel.InitParams = JsonConvert.DeserializeObject<NotificationAction>(extra);
            }

            DrawerLayout = FindViewById<DrawerLayout>(Resource.Id.AppDrawerLayout);

            var set = this.CreateBindingSet<MainActivity, MainViewModel>();
            set.Bind(this).For(v => v.ShowDrawerRequest).To(vm => vm.ShowDrawer).OneWay();
            set.Bind(this).For(v => v.ChangeThemeRequest).To(vm => vm.AppThemeChanged).OneWay();
            set.Bind(this).For(v => v.ChangeLanguageRequest).To(vm => vm.AppLanguageChanged).OneWay();
            set.Bind(this).For(v => v.HideKeyboardRequest).To(vm => vm.HideKeyboard).OneWay();
            set.Bind(this).For(v => v.LockDrawerRequest).To(vm => vm.ShowProgressOverlay).OneWay();
            set.Bind(this).For(v => v.UpdateTaskSortOrderRequest).To(vm => vm.UpdateTaskSortOrder).OneWay();
            set.Apply();

            _selectedTaskSortType = ViewModel.AppSettings.DefaultTaskSortOrder;
            ViewModel.InitViewCommand.Execute();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            var accountsOption = menu.FindItem(Resource.Id.Accounts);
            accountsOption?.SetTitle(ViewModel.GetText("Accounts"));

            var settingsOption = menu.FindItem(Resource.Id.Settings);
            settingsOption?.SetTitle(ViewModel.GetText("Settings"));

            var logoutOption = menu.FindItem(Resource.Id.Logout);
            logoutOption?.SetTitle(ViewModel.GetText("Logout"));

            var fullSyncOption = menu.FindItem(Resource.Id.FullSync);
            fullSyncOption?.SetTitle(ViewModel.GetText("Synchronization"));

            var sortOption = menu.FindItem(Resource.Id.SortTasks);
            sortOption?.SetTitle(ViewModel.GetText("Sort"));

            var sortMenu = sortOption.SubMenu;

            var resourceId = GetSelectedResourceId(_selectedTaskSortType);
            sortMenu.FindItem(Resource.Id.SortByNameAsc).SetTitle(ViewModel.GetText("SortByNameAsc"));
            sortMenu.FindItem(Resource.Id.SortByNameDesc).SetTitle(ViewModel.GetText("SortByNameDesc"));
            sortMenu.FindItem(Resource.Id.SortByUpdatedDateAsc).SetTitle(ViewModel.GetText("SortByUpdatedDateAsc"));
            sortMenu.FindItem(Resource.Id.SortByUpdatedDateDesc).SetTitle(ViewModel.GetText("SortByUpdatedDateDesc"));

            sortMenu.FindItem(resourceId).SetChecked(true);

            return base.OnPrepareOptionsMenu(menu);
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
                case Resource.Id.FullSync:
                    ViewModel.SyncCommand.Execute();
                    break;
                case Resource.Id.SortByNameAsc:
                case Resource.Id.SortByNameDesc:
                case Resource.Id.SortByUpdatedDateAsc:
                case Resource.Id.SortByUpdatedDateDesc:
                    item.SetChecked(true);
                    var sortType = GetSelectedTaskSortType(id);
                    ViewModel.TaskSortOrderChangedCommand.Execute(sortType);
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
            else if (SupportFragmentManager.FindFragmentById(Resource.Id.ContentFrame) is NewTaskFragment fragment &&
                fragment.ViewModel.ChangesWereMade())
            {
                fragment.ViewModel.CloseCommand.Execute();
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

        public void LockDrawer(bool lockDrawer)
        {
            if (lockDrawer)
                DrawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
            else
                DrawerLayout.SetDrawerLockMode(DrawerLayout.LockModeUnlocked);
        }

        public static Intent CreateIntent(int id, string key, string extra)
        {
            var intent = new Intent(Application.Context, typeof(MainActivity))
                .SetAction(nameof(MainActivity) + id);

            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(extra))
            {
                intent.PutExtra(key, extra);
            }

            return intent;
        }

        private TaskSortType GetSelectedTaskSortType(int resourceId)
        {
            switch (resourceId)
            {
                case Resource.Id.SortByNameAsc:
                    return TaskSortType.BY_NAME_ASC;
                case Resource.Id.SortByNameDesc:
                    return TaskSortType.BY_NAME_DESC;
                case Resource.Id.SortByUpdatedDateAsc:
                    return TaskSortType.BY_UPDATED_DATE_ASC;
                case Resource.Id.SortByUpdatedDateDesc:
                    return TaskSortType.BY_UPDATED_DATE_DESC;
                default:
                    throw new ArgumentOutOfRangeException(nameof(resourceId), resourceId, "The provided resource id does not exists");
            }
        }

        private int GetSelectedResourceId(TaskSortType sortType)
        {
            switch (sortType)
            {
                case TaskSortType.BY_NAME_ASC:
                    return Resource.Id.SortByNameAsc;
                case TaskSortType.BY_NAME_DESC:
                    return Resource.Id.SortByNameDesc;
                case TaskSortType.BY_UPDATED_DATE_ASC:
                    return Resource.Id.SortByUpdatedDateAsc;
                case TaskSortType.BY_UPDATED_DATE_DESC:
                    return Resource.Id.SortByUpdatedDateDesc;
                case TaskSortType.CUSTOM_ASC:
                case TaskSortType.CUSTOM_DESC:
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortType), sortType, "The TaskSortType doesnt have a default sort type");
            }
        }

        private void SetSelectedTaskSortType(TaskSortType sortType)
        {
            //For some reason if the SorTask menu options item is visible, it wont automatically trigger the invalidate options
            //when clicked
            InvalidateOptionsMenu();
            _selectedTaskSortType = sortType;
        }
    }
}