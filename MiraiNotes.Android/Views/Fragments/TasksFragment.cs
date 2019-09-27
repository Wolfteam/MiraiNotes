using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Views;
using Android.Widget;
using MiraiNotes.Android.Adapters;
using MiraiNotes.Android.Listeners;
using MiraiNotes.Android.ViewModels;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;

namespace MiraiNotes.Android.Views.Fragments
{
    [MvxFragmentPresentation(typeof(MainViewModel), Resource.Id.ContentFrame)]
    public class TasksFragment : BaseFragment<TasksViewModel>
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
        private SimpleItemTouchHelperCallback _callback;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);

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
            _callback = new SimpleItemTouchHelperCallback(_tasksAdapter);
            var touchHelper = new ItemTouchHelper(_callback);

            _taskRecyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.TaskRecyclerView);
            _taskRecyclerView.AddOnScrollListener(new TasksRecyclerViewScrollListener(_mainFab, CloseSwypedItem));
            _taskRecyclerView.Adapter = _tasksAdapter;
            _taskRecyclerView.AddItemDecoration(new DividerItemDecoration(ParentActivity, LinearLayoutManager.Vertical));

            //_taskRecyclerView.AddItemDecoration(new TaskRecyclerViewDecoration(c =>
            //{
            //    callback.OnDraw(c);
            //}));
            touchHelper.AttachToRecyclerView(_taskRecyclerView);

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

        private void CloseSwypedItem()
        {
            if (_callback.CurrentViewHolder != null)
                _callback.ClearView(_taskRecyclerView, _callback.CurrentViewHolder);
        }
    }
}