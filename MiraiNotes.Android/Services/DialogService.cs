using System;
using Android.App;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using ES.DMoral.ToastyLib;
using MiraiNotes.Android.Dialogs;
using MiraiNotes.Android.Interfaces;

namespace MiraiNotes.Android.Services
{
    public class DialogService : IDialogService
    {
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

        public void ShowSnackBar(View view, string msg, string action, bool? longSnackbar = null)
        {
            var duration = GetSnackbarLength(longSnackbar);
            Snackbar.Make(view, msg, duration).Show();
        }

        public void ShowSnackBar(View view, string msg, string action, Action<View> onClick, bool? longSnackbar = null)
        {
            var duration = GetSnackbarLength(longSnackbar);
            Snackbar.Make(view, msg, duration).SetAction(action, onClick).Show();
        }

        public void ShowLoginDialog(Action<string> onOk, Action onCancel)
        {
            var dialog = new LoginPasswordDialogFragment(onOk, onCancel);
            var fm = dialog.FragmentManager.BeginTransaction();
            dialog.Show(fm, nameof(LoginPasswordDialogFragment));
        }

        private static int GetToastLength(bool longToast)
            => longToast
                ? (int) ToastLength.Long
                : (int) ToastLength.Short;

        private static int GetSnackbarLength(bool? longSnackbar = null)
            => longSnackbar.HasValue
                ? longSnackbar.Value
                    ? Snackbar.LengthLong
                    : Snackbar.LengthShort
                : Snackbar.LengthIndefinite;
    }
}