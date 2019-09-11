﻿using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.Views.Activities;
using MvvmCross.Base;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.ViewModels;
using System;
using System.Linq;

namespace MiraiNotes.Android.Views.Fragments
{
    [MvxFragmentPresentation(typeof(MainViewModel), Resource.Id.MenuFrame)]
    public class MenuFragmet : MvxFragment<MenuViewModel>, NavigationView.IOnNavigationItemSelectedListener
    {
        private NavigationView _navView;
        private IMenuItem _previousMenuItem;

        private IMvxInteraction<string> _onUserImgLoadedRequest;
        private IMvxInteraction _onTaskListsLoadedRequest;

        public IMvxInteraction<string> OnUserImgLoadedRequest
        {
            get => _onUserImgLoadedRequest;
            set
            {
                if (_onUserImgLoadedRequest != null)
                    _onUserImgLoadedRequest.Requested -= OnUserImgLoaded;

                _onUserImgLoadedRequest = value;
                _onUserImgLoadedRequest.Requested += OnUserImgLoaded;
            }
        }

        public IMvxInteraction OnTaskListsLoadedRequest
        {
            get => _onTaskListsLoadedRequest;
            set
            {
                if (_onTaskListsLoadedRequest != null)
                    _onTaskListsLoadedRequest.Requested -= OnTaskListsLoaded;

                _onTaskListsLoadedRequest = value;
                _onTaskListsLoadedRequest.Requested += OnTaskListsLoaded;
            }
        }

        public MainActivity MainActivity
            => (MainActivity)Activity;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(Resource.Layout.MenuView, null);
            _navView = view.FindViewById<NavigationView>(Resource.Id.AppNavView);
            _navView.SetNavigationItemSelectedListener(this);

            var headerLayout = _navView.GetHeaderView(0);
            var circleImg = headerLayout.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.ProfileImg);
            circleImg.Click += (sender, ar) => MainActivity.ViewModel.OnAccountsSelectedCommand.Execute();

            var set = this.CreateBindingSet<MenuFragmet, MenuViewModel>();
            set.Bind(this).For(v => v.OnUserImgLoadedRequest).To(viewModel => viewModel.OnUserProfileImgLoaded).OneWay();
            set.Bind(this).For(v => v.OnTaskListsLoadedRequest).To(viewModel => viewModel.OnTaskListsLoaded).OneWay();
            set.Apply();

            return view;
        }

        public bool OnNavigationItemSelected(IMenuItem menuItem)
        {
            MainActivity.ShowDrawer(false);
            if (menuItem.GroupId == 0)
                _previousMenuItem?.SetChecked(false);

            _previousMenuItem = menuItem;

            if (menuItem.GroupId != 0)
            {
                switch (menuItem.ItemId)
                {
                    case Resource.Id.Logout:
                        MainActivity.ViewModel.LogoutCommand.Execute();
                        break;
                    case Resource.Id.Accounts:
                        MainActivity.ViewModel.OnAccountsSelectedCommand.Execute();
                        break;
                    case Resource.Id.Settings:
                        MainActivity.ViewModel.OnSettingsSelectedCommand.Execute();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(menuItem.ItemId), menuItem.ItemId, "Invalid drawer menu item id");
                }
                return true;
            }

            menuItem.SetCheckable(true);
            menuItem.SetChecked(true);

            ViewModel.OnTaskListSelectedCommand.Execute(menuItem.ItemId);
            return true;
        }

        private async void OnUserImgLoaded(object sender, MvxValueEventArgs<string> eventArgs)
        {
            if (string.IsNullOrEmpty(eventArgs.Value))
                return;
            var headerLayout = _navView.GetHeaderView(0);
            var circleImg = headerLayout.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.ProfileImg);

            using (var img = await MiscellaneousUtils.GetImageBitmapAsync(eventArgs.Value))
            {
                if (img != null)
                    circleImg?.SetImageBitmap(img);
            }
        }

        private void OnTaskListsLoaded(object sender, EventArgs eventArgs)
        {
            _navView.Menu.Clear();
            var menu = _navView.Menu;

            for (int i = 0; i < ViewModel.TaskLists.Count; i++)
            {
                var numberOfTaskView = LayoutInflater.Inflate(Resource.Layout.NumberOfTasks, null);
                var textView = numberOfTaskView.FindViewById<TextView>(Resource.Id.NumberOfTasksBadge);
                var taskList = ViewModel.TaskLists[i];
                textView.Text = $"{taskList.NumberOfTasks}".PadLeft(2, '0');
                //var rightIcon = new TextView(Activity);
                //rightIcon.Text = $"{ViewModel.TaskLists[i].NumberOfTasks}";
                //rightIcon.SetCompoundDrawablesWithIntrinsicBounds(0, 0, Resource.Drawable.ic_edit_black_24dp, 0);

                var menuItem = menu
                    .Add(0, i, i, taskList.Title)
                    .SetIcon(Resource.Drawable.ic_list_black_24dp)
                    .SetChecked(false)
                    .SetActionView(numberOfTaskView);

                if (i != 0)
                    continue;
                menuItem.SetChecked(true).SetCheckable(true);
                _previousMenuItem = menuItem;
            }

            var subMenu = menu.AddSubMenu(1, 100, 100, ViewModel.GetText("Others"));
            subMenu.Add(1, Resource.Id.Accounts, 101, ViewModel.GetText("Accounts")).SetIcon(Resource.Drawable.ic_account_circle_black_24dp);
            subMenu.Add(1, Resource.Id.Settings, 102, ViewModel.GetText("Settings")).SetIcon(Resource.Drawable.ic_settings_black_24dp);
            subMenu.Add(1, Resource.Id.Logout, 103, ViewModel.GetText("Logout")).SetIcon(Resource.Drawable.ic_arrow_back_black_24dp);

            for (int i = 0, count = _navView.ChildCount; i < count; i++)
            {
                var child = _navView.GetChildAt(i);
                if (child == null || !(child is ListView lv))
                    continue;
                ListView menuView = lv;
                HeaderViewListAdapter adapter = (HeaderViewListAdapter)menuView.Adapter;
                BaseAdapter wrapped = (BaseAdapter)adapter.WrappedAdapter;
                wrapped.NotifyDataSetChanged();
            }
            if (ViewModel.TaskLists.Any())
                ViewModel.OnTaskListSelectedCommand.Execute(0);
        }
    }
}