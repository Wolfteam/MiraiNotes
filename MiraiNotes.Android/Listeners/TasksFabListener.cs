using Android.Animation;
using System;

namespace MiraiNotes.Android.Listeners
{
    public class TasksFabListener : Java.Lang.Object, Animator.IAnimatorListener
    {
        private Action _onAnimationEnd;
        public TasksFabListener(Action onAnimationEnd)
        {
            _onAnimationEnd = onAnimationEnd;
        }

        public void OnAnimationCancel(Animator animation)
        {
        }

        public void OnAnimationEnd(Animator animation)
        {
            _onAnimationEnd();
        }

        public void OnAnimationRepeat(Animator animation)
        {
        }

        public void OnAnimationStart(Animator animation)
        {
        }
    }
}