using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using System;

namespace MiraiNotes.Android.Listeners
{
    public class TasksRecyclerViewScrollListener : RecyclerView.OnScrollListener
    {
        private readonly FloatingActionButton _fab;
        private readonly Func<bool> _canScroll;

        public TasksRecyclerViewScrollListener(FloatingActionButton fab, Func<bool> canScroll)
        {
            _fab = fab;
            _canScroll = canScroll;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            if (!_canScroll.Invoke())
                return;
            if (dy < 0 && !_fab.IsShown)
                _fab.Show();
            else if (dy > 0 && _fab.IsShown)
                _fab.Hide();
        }
    }
}