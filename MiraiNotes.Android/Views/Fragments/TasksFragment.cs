using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MiraiNotes.Android.Adapters;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Listeners;
using MiraiNotes.Android.ViewModels;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;

namespace MiraiNotes.Android.Views.Fragments
{
    [MvxFragmentPresentation(typeof(MainViewModel), Resource.Id.ContentFrame, Tag = nameof(TasksFragment))]
    public class TasksFragment : BaseFragment<TasksViewModel>, ISwipeButtonClickListener
    {
        protected override int FragmentId => Resource.Layout.TasksView;
        public bool IsFabOpen = false;

        private LinearLayout _addNewTaskListFabLayout;
        private LinearLayout _addNewTaskFabLayout;

        private FloatingActionButton _mainFab;
        private FloatingActionButton _newTaskListFab;
        private FloatingActionButton _newTaskFab;
        private View _fabBgLayout;
        private MvxRecyclerView _taskRecyclerView;
        private TasksAdapter _tasksAdapter;

        public SwipeCallback SwipeCallback;
        private const int MoveTaskButtonId = 1;
        private const int DeleteTaskButtonId = 2;

        private IMvxInteraction _resetSwipedItemsRequest;
        public IMvxInteraction ResetSwipedItemsRequest
        {
            get => _resetSwipedItemsRequest;
            set
            {
                if (_resetSwipedItemsRequest != null)
                    _resetSwipedItemsRequest.Requested -= (sender, args)
                        => ResetSwipedItems();

                _resetSwipedItemsRequest = value;
                _resetSwipedItemsRequest.Requested += (sender, args)
                        => ResetSwipedItems();
            }
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);

            var set = this.CreateBindingSet<TasksFragment, TasksViewModel>();
            set.Bind(this).For(v => v.ResetSwipedItemsRequest).To(vm => vm.ResetSwipedItems);
            set.Apply();

            var swipeRefreshLayout = view.FindViewById<MvxSwipeRefreshLayout>(Resource.Id.SwipeRefreshLayout);
            swipeRefreshLayout.SetDistanceToTriggerSync(600);
            swipeRefreshLayout.SetSlingshotDistance(200);

            _mainFab = view.FindViewById<FloatingActionButton>(Resource.Id.AppFab);

            _addNewTaskListFabLayout = view.FindViewById<LinearLayout>(Resource.Id.AddNewTaskListFabLayout);
            _addNewTaskFabLayout = view.FindViewById<LinearLayout>(Resource.Id.AddNewTaskFabLayout);

            _newTaskListFab = view.FindViewById<FloatingActionButton>(Resource.Id.AddNewTaskListFab);
            _newTaskFab = view.FindViewById<FloatingActionButton>(Resource.Id.AddNewTaskFab);

            _fabBgLayout = view.FindViewById(Resource.Id.FabBgLayout);

            _fabBgLayout.Click += (sender, args) =>
            {
                if (IsFabOpen)
                    CloseFabMenu();
            };

            _mainFab.Click += (sender, args) =>
            {
                if (IsFabOpen)
                    CloseFabMenu();
                else
                    ShowFabMenu();
            };

            _newTaskListFab.Click += (sender, args)
                => CloseFabMenu();

            _newTaskFab.Click += (sender, args)
                => CloseFabMenu();

            _tasksAdapter = new TasksAdapter((IMvxAndroidBindingContext)BindingContext);

            _taskRecyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.TaskRecyclerView);
            _taskRecyclerView.Adapter = _tasksAdapter;
            _taskRecyclerView.AddItemDecoration(new DividerItemDecoration(ParentActivity, LinearLayoutManager.Vertical));
            _taskRecyclerView.AddOnScrollListener(new TasksRecyclerViewScrollListener(_mainFab));

            string delete = ViewModel.GetText("Delete");
            var white = Color.White;
            var green = new Color(ContextCompat.GetColor(MainActivity, Resource.Color.DarkGreenAccentColorLight));
            var buttons = new List<SwipeButton>
            {
                new SwipeButton(MainActivity,DeleteTaskButtonId ,delete, Resource.Drawable.ic_delete_black_24dp, Color.Red, white, white, listener: this),
                new SwipeButton(MainActivity, MoveTaskButtonId, GetChangeTaskStatusText, Resource.Drawable.ic_done_black_24dp, GetChangeTaskStatusColor, white, white, position: UnderlayButtonPosition.Left, listener: this)
            };

            SwipeCallback = new SwipeCallback(MainActivity, _taskRecyclerView, buttons);
            return view;
        }

        public void ShowFabMenu()
        {
            IsFabOpen = true;

            _addNewTaskFabLayout.Visibility =
                _addNewTaskListFabLayout.Visibility =
                    _fabBgLayout.Visibility = ViewStates.Visible;

            _mainFab.Animate().RotationBy(45).SetDuration(250);

            _addNewTaskFabLayout.Animate().TranslationY(-Resources.GetDimension(Resource.Dimension.standard_75)).SetDuration(250);
            _addNewTaskListFabLayout.Animate().TranslationY(-Resources.GetDimension(Resource.Dimension.standard_125)).SetDuration(250);
        }

        public void CloseFabMenu()
        {
            IsFabOpen = false;
            _fabBgLayout.Visibility = ViewStates.Gone;

            _mainFab.Animate().RotationBy(45).SetDuration(250);

            _addNewTaskFabLayout.Animate().TranslationY(0).SetDuration(250);
            _addNewTaskListFabLayout.Animate().TranslationY(0).SetDuration(250).SetListener(new TasksFabListener(OnFabAnimationEnd));
        }

        private void OnFabAnimationEnd()
        {
            if (!IsFabOpen)
            {
                _addNewTaskFabLayout.Visibility = ViewStates.Gone;
                _addNewTaskListFabLayout.Visibility = ViewStates.Gone;
            }
        }

        public void OnClick(int buttonId, int pos)
        {
            switch (buttonId)
            {
                case DeleteTaskButtonId:
                    ViewModel.SwipeToDeleteCommand.Execute(pos);
                    break;
                case MoveTaskButtonId:
                    ViewModel.SwipeToChangeTaskStatusCommand.Execute(pos);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttonId), buttonId, "The provided buttonId is not valid");
            }
            ResetSwipedItems();
        }

        private Color GetChangeTaskStatusColor(int position)
        {
            var item = ViewModel.Tasks[position];
            return item.IsCompleted
                ? new Color(ContextCompat.GetColor(MainActivity, Resource.Color.LigthBlueAccentColorLight))
                : new Color(ContextCompat.GetColor(MainActivity, Resource.Color.DarkGreenAccentColorLight));
        }

        private string GetChangeTaskStatusText(int position)
        {
            var item = ViewModel.Tasks[position];
            return item.IsCompleted
                ? ViewModel.GetText("Incompleted")
                : ViewModel.GetText("Completed");
        }

        public void ResetSwipedItems()
        {
            SwipeCallback.ResetView();
        }
    }
}