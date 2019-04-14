using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Pages.Dialogs;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace MiraiNotes.UWP.Services
{
    public class CustomDialogService : ICustomDialogService
    {
        public async Task ShowErrorMessageDialogAsync(Exception error, string title)
        {
            await ShowMessageDialogAsync(title, error.Message, "Ok");
        }

        public async Task ShowMessageDialogAsync(string title, string message)
        {
            await ShowMessageDialogAsync(title, message, "OK");
        }

        public async Task ShowMessageDialogAsync(string title, string message, string buttonText)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = buttonText,
                BorderBrush = GetBorderBrush(),
                Background = GetBackgroundBrush(),
            };
            
            await dialog.ShowAsync();
        }

        public async Task<bool?> ShowConfirmationDialogAsync(string title, string message)
        {
            return await ShowConfirmationDialogAsync(title, message, "OK", string.Empty, "Cancel");
        }

        public async Task<bool> ShowConfirmationDialogAsync(string title, string message, string yesButtonText, string noButtonText)
        {
            var result = await ShowConfirmationDialogAsync(title, message, yesButtonText, noButtonText, string.Empty);
            return result ?? false;
        }

        public async Task<bool?> ShowConfirmationDialogAsync(string title, string message, string yesButtonText, string noButtonText, string cancelButtonText)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                BorderBrush = GetBorderBrush(),
                Background = GetBackgroundBrush(),
                //IsPrimaryButtonEnabled = true,
                PrimaryButtonText = yesButtonText,
                //IsSecondaryButtonEnabled = true,
                SecondaryButtonText = noButtonText,
                CloseButtonText = cancelButtonText
            };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.None)
            {
                return null;
            }

            return (result == ContentDialogResult.Primary);
        }

        public async Task<string> ShowInputStringDialogAsync(string title)
        {
            return await ShowInputStringDialogAsync(title, string.Empty);
        }

        public async Task<string> ShowInputStringDialogAsync(string title, string defaultText)
        {
            return await ShowInputStringDialogAsync(title, defaultText, "OK", "Cancel");
        }

        public async Task<string> ShowInputStringDialogAsync(string title, string defaultText, string okButtonText, string cancelButtonText)
        {
            var inputTextBox = new TextBox
            {
                AcceptsReturn = false,
                Height = 32,
                Text = defaultText,
                SelectionStart = defaultText.Length,
                BorderThickness = new Thickness(1),
            };
            var dialog = new ContentDialog
            {
                Content = inputTextBox,
                BorderBrush = GetBorderBrush(),
                Background = GetBackgroundBrush(),
                Title = title,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = okButtonText,
                SecondaryButtonText = cancelButtonText
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                return inputTextBox.Text;
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<string> ShowInputTextDialogAsync(string title)
        {
            return await ShowInputTextDialogAsync(title, string.Empty);
        }

        public async Task<string> ShowInputTextDialogAsync(string title, string defaultText)
        {
            return await ShowInputTextDialogAsync(title, defaultText, "OK", "Cancel");
        }

        public async Task<string> ShowInputTextDialogAsync(string title, string defaultText, string okButtonText, string cancelButtonText)
        {
            var inputTextBox = new TextBox
            {
                AcceptsReturn = true,
                Height = 32 * 6,
                Text = defaultText,
                TextWrapping = TextWrapping.Wrap,
                SelectionStart = defaultText.Length,
                Opacity = 1,
                BorderThickness = new Thickness(1),
            };
            var dialog = new ContentDialog
            {
                Content = inputTextBox,
                Title = title,
                IsSecondaryButtonEnabled = true,
                BorderBrush = GetBorderBrush(),
                Background = GetBackgroundBrush(),
                PrimaryButtonText = okButtonText,
                SecondaryButtonText = cancelButtonText
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                return inputTextBox.Text;
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task<bool> ShowCustomDialog(CustomDialogType dialogType)
        {
            ContentDialog dialog;
            ContentDialogResult result;
            switch (dialogType)
            {
                case CustomDialogType.PASSWORD_DIALOG:
                    dialog = new SettingsPasswordContentDialog();
                    //diaglog.Closing += (sender, args) =>
                    //{
                    //    // This mean user does click on Primary or Secondary button
                    //    if (args.Result == ContentDialogResult.None)
                    //    {
                    //        args.Cancel = true;
                    //    }
                    //};
                    break;
                case CustomDialogType.LOGIN_PASSWORD_DIALOG:
                    dialog = new LoginPasswordContentDialog();
                    break;
                case CustomDialogType.ACCOUNTS_DIALOG:
                    dialog = new AccountsContentDialog();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dialogType), dialogType, "The provided dialog type doesnt exists");
            }
            result = await dialog.ShowAsync();
            if (result == ContentDialogResult.None)
                return false;
            return result == ContentDialogResult.Primary;
        }

        private AcrylicBrush GetBackgroundBrush()
            => Application.Current.Resources["ContentDialogBackground"] as AcrylicBrush;

        private SolidColorBrush GetBorderBrush()
            => Application.Current.Resources["SystemControlBackgroundAccentBrush"] as SolidColorBrush;
    }
}
