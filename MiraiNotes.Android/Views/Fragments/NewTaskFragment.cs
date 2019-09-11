using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using MiraiNotes.Android.Controls;
using MiraiNotes.Android.ViewModels;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using System.Linq;

namespace MiraiNotes.Android.Views.Fragments
{
    [MvxFragmentPresentation(typeof(MainViewModel), Resource.Id.ContentFrame, AddToBackStack = true)]
    public class NewTaskFragment : BaseFragment<NewTaskViewModel>
    {
        protected override int FragmentId => Resource.Layout.NewTaskView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //this is required to populate the options menu from 
            //this fragment. it will call the invalidate options menu of the activity
            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);
            SetActionBarTitle(true);

            //lines below are used to place the icon to the top, otherwise it will appear at the center
            //of the edittext
            var editText = view.FindViewById<AppCompatEditText>(Resource.Id.TaskNotes);
            var innerDrawable = editText.GetCompoundDrawables().First();
            var gravityDrawable = new TopGravityDrawable(innerDrawable);
            innerDrawable.SetBounds(0, 0, innerDrawable.IntrinsicWidth, innerDrawable.IntrinsicHeight);
            gravityDrawable.SetBounds(0, 0, innerDrawable.IntrinsicWidth, innerDrawable.IntrinsicHeight);
            editText.SetCompoundDrawables(gravityDrawable, null, null, null);

            return view;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            //we hide the main options
            menu.Clear();
            inflater.Inflate(Resource.Menu.menu_new_task, menu);
        }

        public override void OnPrepareOptionsMenu(IMenu menu)
        {
            var saveOption = menu.FindItem(Resource.Id.SaveTask);
            saveOption?.SetTitle(ViewModel.GetText("SaveChanges"));

            var discardOption = menu.FindItem(Resource.Id.DiscardChanges);
            discardOption?.SetTitle(ViewModel.GetText("DiscardChanges"));

            var deleteOption = menu.FindItem(Resource.Id.DeleteTask);
            deleteOption?.SetTitle(ViewModel.GetText("Delete"));

            base.OnPrepareOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.SaveTask:
                    ViewModel.SaveChangesCommand.Execute();
                    break;
                case Resource.Id.DiscardChanges:
                    ViewModel.CloseCommand.Execute();
                    break;
                case Resource.Id.DeleteTask:
                    ViewModel.DeleteTaskCommand.Execute();
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}