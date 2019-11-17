using Android.Animation;
using System;

namespace MiraiNotes.Android.Listeners
{
    public class ProgressOverlayListener : Java.Lang.Object, Animator.IAnimatorListener
    {
        private readonly Action _onAnimationEnd;

        public ProgressOverlayListener(Action onAnimationEnd)
        {
            _onAnimationEnd = onAnimationEnd;
        }

        public void OnAnimationCancel(Animator animation)
        {
        }

        public void OnAnimationEnd(Animator animation)
        {
            _onAnimationEnd.Invoke();
        }

        public void OnAnimationRepeat(Animator animation)
        {
        }

        public void OnAnimationStart(Animator animation)
        {
        }
    }
}