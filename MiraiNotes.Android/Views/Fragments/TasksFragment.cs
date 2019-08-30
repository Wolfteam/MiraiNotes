using Android.OS;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using MiraiNotes.Android.Listeners;
using MiraiNotes.Android.ViewModels;
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

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

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

            ParentActivity.SupportActionBar.Title = "Tasks";

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
    }
}