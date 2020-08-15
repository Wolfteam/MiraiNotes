using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using MiraiNotes.Android.Models;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.Views.Fragments;
using MiraiNotes.Core.Enums;
using MvvmCross.Base;
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
        private IMvxInteraction<bool> _changeTasksSelectionModeRequest;
        private bool _lockDrawerRequest;
        private TaskSortType _selectedTaskSortType;
        private bool _allTasksAreSelected;

        public const string InitParamsKey = "InitParams";

        public IMvxInteraction<bool> ShowDrawerRequest
        {
            get => _showDrawerRequest;
            set
            {
                if (_showDrawerRequest != null)
                    _showDrawerRequest.Requested -= ShowDrawerHandler;

                _showDrawerRequest = value;
                _showDrawerRequest.Requested += ShowDrawerHandler;
            }
        }
        public IMvxInteraction<AppThemeChangedMsg> ChangeThemeRequest
        {
            get => _changeThemeRequest;
            set
            {
                if (_changeThemeRequest != null)
                    _changeThemeRequest.Requested -= SetAppThemeHandler;

                _changeThemeRequest = value;
                _changeThemeRequest.Requested += SetAppThemeHandler;
            }
        }

        public IMvxInteraction ChangeLanguageRequest
        {
            get => _changeLanguageRequest;
            set
            {
                if (_changeLanguageRequest != null)
                    _changeLanguageRequest.Requested -= ChangeLanguageHandler;

                _changeLanguageRequest = value;
                _changeLanguageRequest.Requested += ChangeLanguageHandler;
            }
        }

        public IMvxInteraction HideKeyboardRequest
        {
            get => _hideKeyboardRequest;
            set
            {
                if (_hideKeyboardRequest != null)
                    _hideKeyboardRequest.Requested -= HideKeyboardHandler;

                _hideKeyboardRequest = value;
                _hideKeyboardRequest.Requested += HideKeyboardHandler;
            }
        }

        public IMvxInteraction<TaskSortType> UpdateTaskSortOrderRequest
        {
            get => _updateTaskSortOrderRequest;
            set
            {
                if (_updateTaskSortOrderRequest != null)
                    _updateTaskSortOrderRequest.Requested -= SetSelectedTaskSortTypeHandler;

                _updateTaskSortOrderRequest = value;
                _updateTaskSortOrderRequest.Requested += SetSelectedTaskSortTypeHandler;
            }
        }

        public IMvxInteraction<bool> ChangeTasksSelectionModeRequest
        {
            get => _changeTasksSelectionModeRequest;
            set
            {
                if (_changeTasksSelectionModeRequest != null)
                    _changeTasksSelectionModeRequest.Requested -= StartSelectionModeHandler;

                _changeTasksSelectionModeRequest = value;
                _changeTasksSelectionModeRequest.Requested += StartSelectionModeHandler;
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
            set.Bind(this).For(v => v.ChangeTasksSelectionModeRequest).To(vm => vm.ChangeTasksSelectionMode).OneWay();
            set.Apply();

            _selectedTaskSortType = ViewModel.AppSettings.DefaultTaskSortOrder;
            bool orientationChanged = savedInstanceState != null;
            ViewModel.InitViewCommand.Execute(orientationChanged);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var fragment = SupportFragmentManager.FindFragmentById(Resource.Id.ContentFrame) as TasksFragment;
            if (!ViewModel.IsInSelectionMode)
            {
                fragment?.IsActionBarBackButtonEnabled(false);
                MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            }
            else
            {
                fragment?.IsActionBarBackButtonEnabled(true);
                MenuInflater.Inflate(Resource.Menu.menu_selection_mode, menu);
            }
            return true;
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            if (!ViewModel.IsInSelectionMode)
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

                var manageTaskLists = menu.FindItem(Resource.Id.ManageTaskLists);
                manageTaskLists?.SetTitle(ViewModel.GetText("ManageTaskLists"));

                var selectionMode = menu.FindItem(Resource.Id.SelectionMode);
                selectionMode?.SetTitle(ViewModel.GetText("SelectionMode"));

                var sortMenu = sortOption?.SubMenu;

                var resourceId = GetSelectedResourceId(_selectedTaskSortType);
                sortMenu?.FindItem(Resource.Id.SortByNameAsc)?.SetTitle(ViewModel.GetText("SortByNameAsc"));
                sortMenu?.FindItem(Resource.Id.SortByNameDesc)?.SetTitle(ViewModel.GetText("SortByNameDesc"));
                sortMenu?.FindItem(Resource.Id.SortByUpdatedDateAsc)?.SetTitle(ViewModel.GetText("SortByUpdatedDateAsc"));
                sortMenu?.FindItem(Resource.Id.SortByUpdatedDateDesc)?.SetTitle(ViewModel.GetText("SortByUpdatedDateDesc"));

                sortMenu?.FindItem(resourceId)?.SetChecked(true);
            }
            else
            {
                var deleteSelectedTasksOptions = menu.FindItem(Resource.Id.DeleteSelectedTasks);
                deleteSelectedTasksOptions?.SetTitle(ViewModel.GetText("DeleteSelectedTasks"));

                var markSelectedTasksAsCompletedOption = menu.FindItem(Resource.Id.MarkAsCompletedSelectedTasks);
                markSelectedTasksAsCompletedOption?.SetTitle(ViewModel.GetText("MarkSelectedTasksAsCompleted"));

                var moveSelectedTasksOption = menu.FindItem(Resource.Id.MoveSelectedTasks);
                moveSelectedTasksOption?.SetTitle(ViewModel.GetText("MoveSelectedTasks"));

                var selectAllTasksOption = menu.FindItem(Resource.Id.SelectAllTasks);
                selectAllTasksOption?.SetTitle(ViewModel.GetText("SelectUnselectAllTasks"));
            }

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
                    ViewModel.LogoutCommand.Execute();
                    break;
                case Resource.Id.FullSync:
                    ViewModel.SyncCommand.Execute();
                    break;
                case Resource.Id.ManageTaskLists:
                    ViewModel.ManageTaskListsCommand.Execute();
                    break;
                case Resource.Id.SortByNameAsc:
                case Resource.Id.SortByNameDesc:
                case Resource.Id.SortByUpdatedDateAsc:
                case Resource.Id.SortByUpdatedDateDesc:
                    item.SetChecked(true);
                    var sortType = GetSelectedTaskSortType(id);
                    ViewModel.TaskSortOrderChangedCommand.Execute(sortType);
                    break;
                //Below cases are only visible on selection mode
                case Resource.Id.SelectionMode:
                    StartSelectionMode(true);
                    break;
                case Resource.Id.SelectAllTasks:
                    {
                        _allTasksAreSelected = !_allTasksAreSelected;
                        var tasksFragment = SupportFragmentManager.FindFragmentById(Resource.Id.ContentFrame) as TasksFragment;
                        tasksFragment?.ViewModel.SelectAllTasksCommand.Execute(_allTasksAreSelected);
                    }
                    break;
                case Resource.Id.DeleteSelectedTasks:
                    {
                        var tasksFragment = SupportFragmentManager.FindFragmentById(Resource.Id.ContentFrame) as TasksFragment;
                        tasksFragment?.ViewModel.DeleteSelectedTasksCommand.Execute();
                    }
                    break;
                case Resource.Id.MarkAsCompletedSelectedTasks:
                    {
                        var tasksFragment = SupportFragmentManager.FindFragmentById(Resource.Id.ContentFrame) as TasksFragment;
                        tasksFragment?.ViewModel.MarkSelectedTasksAsCompletedCommand.Execute();
                    }
                    break;
                case Resource.Id.MoveSelectedTasks:
                    {
                        var tasksFragment = SupportFragmentManager.FindFragmentById(Resource.Id.ContentFrame) as TasksFragment;
                        tasksFragment?.ViewModel.MoveSelectedTasksCommand.Execute();
                    }
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
                (tasksFragment.IsFabOpen || tasksFragment.SwipeCallback.AnItemIsOpen))
            {
                if (tasksFragment.IsFabOpen)
                    tasksFragment.CloseFabMenu();
                else if (tasksFragment.SwipeCallback.AnItemIsOpen)
                    tasksFragment.ResetSwipedItems();
            }
            else if (SupportFragmentManager.FindFragmentById(Resource.Id.ContentFrame) is NewTaskFragment fragment &&
                fragment.ViewModel.ChangesWereMade())
            {
                fragment.ViewModel.CloseCommand.Execute();
            }
            else if (ViewModel.IsInSelectionMode)
            {
                StartSelectionMode(false);
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
            DrawerLayout.SetDrawerLockMode(lockDrawer
                ? DrawerLayout.LockModeLockedClosed
                : DrawerLayout.LockModeUnlocked);
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
            return resourceId switch
            {
                Resource.Id.SortByNameAsc => TaskSortType.BY_NAME_ASC,
                Resource.Id.SortByNameDesc => TaskSortType.BY_NAME_DESC,
                Resource.Id.SortByUpdatedDateAsc => TaskSortType.BY_UPDATED_DATE_ASC,
                Resource.Id.SortByUpdatedDateDesc => TaskSortType.BY_UPDATED_DATE_DESC,
                _ => throw new ArgumentOutOfRangeException(nameof(resourceId), resourceId, "The provided resource id does not exists"),
            };
        }

        private int GetSelectedResourceId(TaskSortType sortType)
        {
            return sortType switch
            {
                TaskSortType.BY_NAME_ASC => Resource.Id.SortByNameAsc,
                TaskSortType.BY_NAME_DESC => Resource.Id.SortByNameDesc,
                TaskSortType.BY_UPDATED_DATE_ASC => Resource.Id.SortByUpdatedDateAsc,
                TaskSortType.BY_UPDATED_DATE_DESC => Resource.Id.SortByUpdatedDateDesc,
                _ => throw new ArgumentOutOfRangeException(nameof(sortType), sortType, "The TaskSortType doesnt have a default sort type"),
            };
        }

        public void ShowDrawerHandler(object sender, MvxValueEventArgs<bool> args)
        {
            ShowDrawer(args.Value);
        }

        private void ChangeLanguageHandler(object sender, EventArgs e)
        {
            RestartActivity();
        }

        private void HideKeyboardHandler(object sender, EventArgs e)
        {
            HideSoftKeyboard();
        }

        private void SetSelectedTaskSortTypeHandler(object sender, MvxValueEventArgs<TaskSortType> args)
        {
            //For some reason if the SorTask menu options item is visible, it wont automatically trigger the invalidate options
            //when clicked
            InvalidateOptionsMenu();
            _selectedTaskSortType = args.Value;
        }

        private void StartSelectionModeHandler(object sender, MvxValueEventArgs<bool> args)
        {
            StartSelectionMode(args.Value);
        }

        private void SetAppThemeHandler(object sender, MvxValueEventArgs<AppThemeChangedMsg> args)
        {
            SetAppTheme(args.Value.AppTheme, args.Value.AccentColor, args.Value.UseDarkAmoledTheme, args.Value.RestartActivity);
        }

        private void StartSelectionMode(bool isInSelectionMode)
        {
            LockDrawer(isInSelectionMode);
            ViewModel.IsInSelectionMode = isInSelectionMode;
            if (SupportFragmentManager.FindFragmentById(Resource.Id.ContentFrame) is TasksFragment f)
            {
                f.ViewModel.IsInSelectionMode = isInSelectionMode;
                f.ViewModel.StartSelectionModeCommand.Execute(isInSelectionMode);
                f.ShowMainFab(!isInSelectionMode);
                f.EnableSwipeToRefresh(!isInSelectionMode);
                f.ResetSwipedItems();
                f.SwipeCallback.IsSwipeEnabled = !isInSelectionMode;
            }
            _allTasksAreSelected = false;
            InvalidateOptionsMenu();
        }
    }
}