using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using MiraiNotes.Android.Listeners;
using System;
using static Android.App.ActivityManager;
using static Android.Graphics.Bitmap;

namespace MiraiNotes.Android.Common.Utils
{
    public static class AndroidUtils
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

        public static Bitmap ToBitmap(this Drawable drawable, int width = 24, int height = 24)
        {
            try
            {
                if (drawable is BitmapDrawable bitmapDrawable)
                {
                    return bitmapDrawable.Bitmap;
                }
                else if (drawable is GradientDrawable gradientDrawable)
                {
                    int w = drawable.IntrinsicWidth > 0
                        ? drawable.IntrinsicWidth
                        : width;
                    int h = drawable.IntrinsicHeight > 0
                        ? drawable.IntrinsicHeight
                        : height;

                    var bitmap = CreateBitmap(w, h, Config.Argb8888);
                    var canvas = new Canvas(bitmap);
                    gradientDrawable.SetBounds(0, 0, w, h);
                    gradientDrawable.SetStroke(1, Color.Black);
                    gradientDrawable.SetFilterBitmap(true);
                    gradientDrawable.Draw(canvas);
                    return bitmap;
                }
                else
                {
                    var bitmap = CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, Config.Argb8888);
                    Canvas canvas = new Canvas(bitmap);
                    drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
                    drawable.Draw(canvas);
                    return bitmap;
                }
            }
            catch (System.Exception e)
            {
                throw;
            }
        }

        public static void SetMinDate(this DatePicker picker, DateTime dt)
        {
            var javaMinDt = new DateTime(1970, 1, 1);
            if (dt.CompareTo(javaMinDt) < 0)
                throw new ArgumentException("Must be >= Java's min DateTime of 1/1970", nameof(dt));

            var longVal = dt - javaMinDt;
            picker.MinDate = (long)longVal.TotalMilliseconds;
        }

        public static void SetMaxDate(this DatePicker picker, DateTime dt)
        {
            var javaMinDt = new DateTime(1970, 1, 1);
            if (dt.CompareTo(javaMinDt) < 0)
                throw new ArgumentException("Must be >= Java's min DateTime of 1/1970", nameof(dt));

            var longVal = dt - javaMinDt;
            picker.MaxDate = (long)longVal.TotalMilliseconds;
        }
    }
}