using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MiraiNotes.Android.ViewModels;
using MvvmCross.Platforms.Android.Presenters.Attributes;

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
            //ParentActivity.SupportActionBar.InvalidateOptionsMenu();
            ParentActivity.SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            return view;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            //we hide the main options
            menu.Clear();
            inflater.Inflate(Resource.Menu.menu_new_task, menu);
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