using MiraiNotes.UWP.ViewModels.Dialogs;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MiraiNotes.UWP.Pages.Dialogs
{
    public sealed partial class AccountsContentDialog : ContentDialog
    {
        public AccountsContentDialog()
        {
            this.InitializeComponent();

            var vm = DataContext as AccountsDialogViewModel;
            vm.HideDialogRequest = () => AccountsDialog.Hide();
        }
    }
}
