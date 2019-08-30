using System;

namespace MiraiNotes.Android.Interfaces
{
    public interface IDialogService
    {
        void ShowInfoToast(string message, bool longToast = false);
        void ShowErrorToast(string message, bool longToast = false);
        void ShowWarningToast(string message, bool longToast = false);
        void ShowSucceedToast(string message, bool longToast = false);

        void ShowSnackBar(string msg, string action = "", bool displayOnContentFrame = true, bool? longSnackbar = false);
        void ShowSnackBar(string msg, Action onClick, string action = "", bool displayOnContentFrame = true, bool? longSnackbar = false);

        void ShowLoginDialog(Action<string> onOk = null, Action onCancel = null);

        void ShowDialog(
            string title,
            string msg,
            string yesButtonText,
            string cancelButtonText,
            Action onOk = null,
            Action onCancel = null);
    }
}