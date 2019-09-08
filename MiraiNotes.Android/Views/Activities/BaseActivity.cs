using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views.InputMethods;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Core.Enums;
using MiraiNotes.Shared;
using MvvmCross.Droid.Support.V7.AppCompat;
using System;

namespace MiraiNotes.Android.Views.Activities
{
    public abstract class BaseActivity<T> : MvxAppCompatActivity<T>
        where T : BaseViewModel
    {
        public abstract int LayoutId { get; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetAppTheme(ViewModel.CurrentAppTheme, ViewModel.CurrentHexAccentColor);
            SetContentView(LayoutId);
        }

        public override bool OnSupportNavigateUp()
        {
            OnBackPressed();
            return true;
        }

        public void SetActionBar(string title, bool isBackEnabled)
        {
            var toolbar = FindViewById<Toolbar>(Resource.Id.AppToolbar);
            if (toolbar != null)
            {
                toolbar.ShowOverflowMenu();
                toolbar.ShowContextMenu();
                SetSupportActionBar(toolbar);
                SupportActionBar.Title = title;
                SupportActionBar.SetDisplayHomeAsUpEnabled(isBackEnabled);
            }
        }

        public void SetAppTheme(
            AppThemeType appTheme,
            string appHexAccentColor,
            bool restartActivity = false)
        {
            appHexAccentColor = appHexAccentColor.ToLower();
            if (appTheme == AppThemeType.DARK)
            {
                switch (appHexAccentColor)
                {
                    case AppConstants.AccentColorLightBlue:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_LightBlue);
                        break;
                    case AppConstants.AccentColorLimeGreen:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_LimeGreen);
                        break;
                    case AppConstants.AccentColorDarkOrange:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_DarkOrange);
                        break;
                    case AppConstants.AccentColorVividRed:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_VividRed);
                        break;
                    case AppConstants.AccentColorDarkCyan:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_DarkCyan);
                        break;
                    case AppConstants.AccentColorDarkGreen:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_DarkGreen);
                        break;
                    case AppConstants.AccentColorDarkMagenta:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_DarkMagenta);
                        break;
                    case AppConstants.AccentColorMagenta:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_Magenta);
                        break;
                    case AppConstants.AccentColorDarkGray:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_DarkGray);
                        break;
                    case AppConstants.AccentColorOrange:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_Orange);
                        break;
                    case AppConstants.AccentColorYellow:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_Yellow);
                        break;
                    case AppConstants.AccentColorDarkBlue:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_DarkBlue);
                        break;
                    case AppConstants.AccentColorViolet:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_Violet);
                        break;
                    case AppConstants.AccentColorLightGrey:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Dark_LightGrey);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(appHexAccentColor), appHexAccentColor, "Invalid accent theme");
                }
            }
            else
            {
                switch (appHexAccentColor)
                {
                    case AppConstants.AccentColorLightBlue:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_LightBlue);
                        break;
                    case AppConstants.AccentColorLimeGreen:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_LimeGreen);
                        break;
                    case AppConstants.AccentColorDarkOrange:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_DarkOrange);
                        break;
                    case AppConstants.AccentColorVividRed:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_VividRed);
                        break;
                    case AppConstants.AccentColorDarkCyan:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_DarkCyan);
                        break;
                    case AppConstants.AccentColorDarkGreen:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_DarkGreen);
                        break;
                    case AppConstants.AccentColorDarkMagenta:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_DarkMagenta);
                        break;
                    case AppConstants.AccentColorMagenta:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_Magenta);
                        break;
                    case AppConstants.AccentColorDarkGray:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_DarkGray);
                        break;
                    case AppConstants.AccentColorOrange:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_Orange);
                        break;
                    case AppConstants.AccentColorYellow:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_Yellow);
                        break;
                    case AppConstants.AccentColorDarkBlue:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_DarkBlue);
                        break;
                    case AppConstants.AccentColorViolet:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_Violet);
                        break;
                    case AppConstants.AccentColorLightGrey:
                        SetTheme(Resource.Style.Theme_MiraiNotes_Light_LightGrey);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(appHexAccentColor), appHexAccentColor, "Invalid accent theme");
                }

            }

            if (!restartActivity)
                return;

            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            Finish();
            StartActivity(intent);
        }

        public void HideSoftKeyboard()
        {
            if (CurrentFocus == null)
                return;

            var inputMethodManager = (InputMethodManager)GetSystemService(InputMethodService);
            inputMethodManager.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);

            CurrentFocus.ClearFocus();
        }
    }
}