using Android.Animation;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using MiraiNotes.Android.Listeners;
using System;
using System.Linq;
using static Android.App.ActivityManager;
using static Android.Graphics.Bitmap;
using AndroidUtil = Android.Util;

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

        public static float ToPixel(float dp, Context context)
        {
            var dmd = (float)AndroidUtil.DisplayMetricsDensity.Default;
            return dp * ((float)context.Resources.DisplayMetrics.DensityDpi / dmd);
        }

        public static float ToDp(float px, Context context)
        {
            var dmd = (float)AndroidUtil.DisplayMetricsDensity.Default;
            return px / ((float)context.Resources.DisplayMetrics.DensityDpi / dmd);
        }

        public static void StartForegroundServiceCompat<T>(this Context context, Bundle args = null)
            where T : Service
        {
            var intent = new Intent(context, typeof(T));
            if (args != null)
            {
                intent.PutExtras(args);
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                context.StartForegroundService(intent);
            }
            else
            {
                context.StartService(intent);
            }
        }

        public static Animator CreateSlideAnimator(
            Context context, 
            int animeResId, 
            float to = 0)
        {
            float deviceWidth = context.Resources.DisplayMetrics.WidthPixels;
            var animSet = AnimatorInflater.LoadAnimator(context, animeResId) as AnimatorSet;
            var anim = animSet.ChildAnimations.First() as ObjectAnimator;
            deviceWidth += (float)(deviceWidth * 0.1);

            switch (animeResId)
            {
                case Resource.Animator.slide_enter_left_to_right:
                    anim.SetFloatValues(-deviceWidth, to);
                    break;
                case Resource.Animator.slide_enter_right_to_left:
                    anim.SetFloatValues(deviceWidth, to);
                    break;
                case Resource.Animator.slide_exit_left_to_right:
                    anim.SetFloatValues(to, deviceWidth);
                    break;
                case Resource.Animator.slide_exit_right_to_left:
                    anim.SetFloatValues(to, -deviceWidth);
                    break;
                default:
                    throw new IndexOutOfRangeException("The provided anim resource is not valid");
            }
            return animSet;
        }
    }
}