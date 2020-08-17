using Android.Graphics;
using AndroidX.RecyclerView.Widget;
using System;

namespace MiraiNotes.Android.Listeners
{
    public class TaskRecyclerViewDecoration : RecyclerView.ItemDecoration
    {
        private readonly Action<Canvas> _onDraw;

        public TaskRecyclerViewDecoration(Action<Canvas> onDraw)
        {
            _onDraw = onDraw;
        }

        public override void OnDraw(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            _onDraw(c);
        }
    }
}