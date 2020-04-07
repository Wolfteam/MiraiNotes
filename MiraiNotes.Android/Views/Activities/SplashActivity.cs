using Android.App;
using MvvmCross.Droid.Support.V7.AppCompat;

namespace MiraiNotes.Android.Views.Activities
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Theme = "@style/Theme.Splash",
        NoHistory = true)]
    public class SplashActivity : MvxSplashScreenAppCompatActivity<Setup, App>
    {
        public SplashActivity()
        {
        }
    }
}