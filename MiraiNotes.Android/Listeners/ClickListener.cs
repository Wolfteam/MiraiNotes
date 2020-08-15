using Android.Views;
using System;

namespace MiraiNotes.Android.Listeners
{
    public class ClickListener : Java.Lang.Object, View.IOnClickListener
    {
        private readonly Action<View> _action;

        public ClickListener(Action<View> action)
        {
            _action = action;
        }

        public void OnClick(View v)
        {
            _action(v);
        }
    }
}