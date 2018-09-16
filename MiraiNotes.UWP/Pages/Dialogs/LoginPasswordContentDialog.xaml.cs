using MiraiNotes.UWP.ViewModels.Dialogs;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MiraiNotes.UWP.Pages.Dialogs
{
    public sealed partial class LoginPasswordContentDialog : ContentDialog
    {
        private LoginPasswordDialogViewModel ViewModel => DataContext as LoginPasswordDialogViewModel;

        public LoginPasswordContentDialog()
        {
            this.InitializeComponent();
            this.PrimaryButtonClick += async (sender, args) =>
            {
                var deferral = args.GetDeferral();
                bool matches = await ViewModel.PasswordMatches();

                if (!matches)
                    args.Cancel = true;

                deferral.Complete();
            };
        }
    }
}
