using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using System;

namespace MiraiNotes.Android.Listeners
{
    public class TasksRecyclerViewScrollListener : RecyclerView.OnScrollListener
    {
        private readonly FloatingActionButton _fab;
        private readonly Action _onScrolled;

        public TasksRecyclerViewScrollListener(FloatingActionButton fab, Action onScrolled)
        {
            _fab = fab;
            _onScrolled = onScrolled;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            if (dy < 0 && !_fab.IsShown)
                _fab.Show();
            else if (dy > 0 && _fab.IsShown)
                _fab.Hide();
        }

        public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
        {
            base.OnScrollStateChanged(recyclerView, newState);

            if (newState == RecyclerView.ScrollStateDragging)
            {
                _onScrolled.Invoke();
            }
        }
    }
}