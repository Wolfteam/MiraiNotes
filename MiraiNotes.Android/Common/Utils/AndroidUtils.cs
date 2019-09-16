using Android.App;
using Android.Views;
using MiraiNotes.Android.Listeners;
using static Android.App.ActivityManager;

namespace MiraiNotes.Android.Common.Utils
{
    public class AndroidUtils
    {
        public static void AnimateView(View view, int toVisibility, float toAlpha, int duration)
        {
            bool show = toVisibility == (int)ViewStates.Visible;
            if (show)
            {
                view.Alpha = 0;
            }
            view.Visibility = ViewStates.Visible;
            view.Animate()
                .SetDuration(duration)
                .Alpha(show ? toAlpha : 0)
                .SetListener(new ProgressOverlayListener(() =>
                {
                    view.Visibility = (ViewStates)toVisibility;
                }));
        }

        public static bool IsAppInForeground()
        {
            var myProcess = new RunningAppProcessInfo();
            GetMyMemoryState(myProcess);
            return myProcess.Importance == Importance.Foreground;
        }
    }
}