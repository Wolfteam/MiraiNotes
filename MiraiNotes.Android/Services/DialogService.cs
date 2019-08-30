using Android.App;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using ES.DMoral.ToastyLib;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Views.Fragments.Dialogs;
using MvvmCross;
using MvvmCross.Platforms.Android;
using System;

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

        public void ShowSnackBar(string msg, string action = "", bool displayOnContentFrame = true, bool? longSnackbar = false)
        {
            var view = GetSnackbarView(displayOnContentFrame);
            var duration = GetSnackbarLength(longSnackbar);
            Snackbar.Make(view, msg, duration).Show();
        }

        public void ShowSnackBar(string msg, Action onClick, string action = "", bool displayOnContentFrame = true, bool? longSnackbar = false)
        {
            var view = GetSnackbarView(displayOnContentFrame);
            var duration = GetSnackbarLength(longSnackbar);
            Snackbar.Make(view, msg, duration).SetAction(action, (v) => onClick.Invoke()).Show();
        }

        public void ShowLoginDialog(Action<string> onOk = null, Action onCancel = null)
        {
            var dialog = new LoginPasswordDialogFragment(onOk, onCancel);
            var top = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            var activity = top.Activity as MainActivity;
            dialog.Show(activity.SupportFragmentManager, nameof(LoginPasswordDialogFragment));
        }


        public void ShowDialog(
            string title,
            string msg,
            string yesButtonText,
            string cancelButtonText,
            Action onOk = null,
            Action onCancel = null)
        {
            var top = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();

            var dialog = new AlertDialog.Builder(top.Activity)
                .SetMessage(msg)
                .SetTitle(title)
                .SetPositiveButton(yesButtonText, (sender, args) => onOk?.Invoke())
                .SetNegativeButton(cancelButtonText, (sender, args) => onCancel?.Invoke())
                .Create();

            dialog.Show();
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

        private View GetSnackbarView(bool displayOnContentFrame)
        {
            var top = Mvx.IoCProvider.Resolve<IMvxAndroidCurrentTopActivity>();
            var view = top.Activity.FindViewById(displayOnContentFrame
                ? Resource.Id.ContentFrame
                : Resource.Id.TaskViewLayout);

            return view;
        }
    }
}