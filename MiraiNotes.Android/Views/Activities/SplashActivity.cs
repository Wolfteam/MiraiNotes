using Android.App;
using MvvmCross.Platforms.Android.Views;

namespace MiraiNotes.Android.Views.Activities
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Theme = "@style/Theme.Splash",
        NoHistory = true)]
    public class SplashActivity : MvxSplashScreenActivity<Setup, App>
    {
    }
}