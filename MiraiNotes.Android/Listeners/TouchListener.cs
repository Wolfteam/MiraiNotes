using Android.Views;
using System;

namespace MiraiNotes.Android.Listeners
{
    public class TouchListener : Java.Lang.Object, View.IOnTouchListener
    {
        private readonly Func<View, MotionEvent, bool> _action;

        public TouchListener(Func<View, MotionEvent, bool> action)
        {
            _action = action;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            return _action.Invoke(v, e);
        }
    }
}