using System;
using Android.Views;

namespace MiraiNotes.Android.Interfaces
{
    public interface IDialogService
    {
        void ShowInfoToast(string message, bool longToast = false);
        void ShowErrorToast(string message, bool longToast = false);
        void ShowWarningToast(string message, bool longToast = false);
        void ShowSucceedToast(string message, bool longToast = false);

        void ShowSnackBar(View view, string msg, string action, bool? longSnackbar = null);

        void ShowSnackBar(View view, string msg, string action, Action<View> onClick, bool? longSnackbar = null);
        void ShowLoginDialog(Action<string> onOk, Action onCancel);
    }
}