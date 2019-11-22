using MiraiNotes.UWP.ViewModels;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MiraiNotes.UWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewTaskPage : Page
    {
        public NewTaskPageViewModel ViewModel
            => DataContext as NewTaskPageViewModel;

        public NewTaskPage()
        {
            this.InitializeComponent();
        }

        private void TaskTitle_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string key = nameof(TaskItemViewModel.Title);

            if (!ViewModel.InitialValues.ContainsKey(key)) //this one is because for some reason linebreaks are replaced by \r
                ViewModel.InitialValues.Add(key, string.IsNullOrEmpty(textBox.Text) ? string.Empty : textBox.Text);
            else
                ViewModel.TextChanged(key, textBox.Text);
        }

        private void TaskBody_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string key = nameof(TaskItemViewModel.Notes);

            if (!ViewModel.InitialValues.ContainsKey(key)) //this one is because for some reason linebreaks are replaced by \r
                ViewModel.InitialValues.Add(key, string.IsNullOrEmpty(textBox.Text) ? string.Empty : textBox.Text);
            else
                ViewModel.TextChanged(key, textBox.Text);
        }
    }
}
