using Android.Views;

namespace MiraiNotes.Android.Listeners
{
    public class SimpleOnGestureListener : GestureDetector.SimpleOnGestureListener
    {
        private readonly SwipeCallbackBase _swipeHelper;

        public SimpleOnGestureListener(SwipeCallbackBase swipeHelper)
        {
            _swipeHelper = swipeHelper;
        }

        public override bool OnSingleTapConfirmed(MotionEvent e)
        {
            foreach (var item in _swipeHelper.Buttons)
            {
                if (item.OnClick(e.GetX(), e.GetY()))
                {
                    break;
                }
            }

            return true;
        }
    }
}