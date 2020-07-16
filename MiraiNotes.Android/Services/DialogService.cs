using Android.App;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using ES.DMoral.ToastyLib;
using Google.Android.Material.Snackbar;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MvvmCross;
using MvvmCross.Platforms.Android;
using System;

namespace MiraiNotes.Android.Services
{
    public class DialogService : IDialogService
    {
        private readonly IAppSettingsService _appSettings;

        public DialogService(IAppSettingsService appSettings)
        {
            _appSettings = appSettings;
        }

        public void ShowInfoToast(string message, bool longToast = false)
        {
            var toastLength = GetToastLength(longToast);
            Toasty.Info(Application.Context, message, toastLength, true).Show();
        }

        public void ShowErrorToast(string message, bool longToast = false)
        {
            var toastLength = GetToastLength(longToast);
            Toasty.Error(Application.Context, message, toastLength, true).Show();
        }

        public void ShowWarningToast(string message, bool longToast = false)
        {
            var toastLength = GetToastLength(longToast);
            Toasty.Warning(Application.Context, message, toastLength, true).Show();
        }

        public void ShowSucceedToast(string message, bool longToast = false)
        {
            var toastLength = GetToastLength(longToast);
            Toasty.Success(Application.Context, message, toastLength, true).Show();
        }

        public void ShowSnackBar(string msg, string action = "", bool? longSnackbar = false)
        {
            var ssb = FormatSnackbarText(msg);
            var view = GetSnackbarView();
            var duration = GetSnackbarLength(longSnackbar);
            var snackbar = Snackbar.Make(view, ssb, duration);
            if (!string.IsNullOrEmpty(action))
            {
                var color = Color.ParseColor(_appSettings.AppHexAccentColor);
                snackbar.SetActionTextColor(color);
            }
            snackbar.Show();
        }

        public void ShowSnackBar(string msg, Action onClick, string action = "", bool? longSnackbar = false)
        {
            var ssb = FormatSnackbarText(msg);
            var view = GetSnackbarView();
            var duration = GetSnackbarLength(longSnackbar);
            var snackbar = Snackbar.Make(view, ssb, duration)
                .SetAction(action, (v) => onClick.Invoke())
                .SetActionTextColor(Color.Blue);

            if (!string.IsNullOrEmpty(action))
            {
                var color = Color.ParseColor(_appSettings.AppHexAccentColor);
                snackbar.SetActionTextColor(color);
            }
            snackbar.Show();
        }

        private static int GetToastLength(bool longToast)
            => longToast
                ? (int)ToastLength.Long
                : (int)ToastLength.Short;

        private static int GetSnackbarLength(bool? longSnackbar = null)
            => longSnackbar.HasValue
                ? longSnackbar.Value
                    ? Snackbar.LengthLong
                    : Snackbar.LengthShort
                : Snackbar.LengthIndefinite;

        private View GetSnackbarView()
        {
            var top = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            var view = top.Activity.FindViewById(Resource.Id.ContentFrame);
            return view;
        }

        private SpannableStringBuilder FormatSnackbarText(string text)
        {
            SpannableStringBuilder ssb = new SpannableStringBuilder();
            ssb.Append(text);
            ssb.SetSpan(
                new ForegroundColorSpan(Color.White),
                0,
                text.Length,
                SpanTypes.ExclusiveExclusive);
            return ssb;
        }
    }
}