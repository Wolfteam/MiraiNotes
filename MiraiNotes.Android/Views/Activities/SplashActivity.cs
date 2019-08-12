﻿using Android.App;
using Android.Content.PM;
using MvvmCross.Droid.Support.V7.AppCompat;

namespace MiraiNotes.Android.Views.Activities
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Theme = "@style/Theme.Splash",
        NoHistory = true,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : MvxSplashScreenAppCompatActivity<Setup<App>, App>
    {
        public SplashActivity()
        {
        }
    }
}