using MiraiNotes.UWP.Pages;
using MiraiNotes.UWP.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MiraiNotes.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const double PANE_WIDTH_PERCENTAGE = .45;

        public MainPage()
        {
            this.InitializeComponent();
            ContentFrame.Navigate(typeof(TasksPage));
            PaneFrame.Navigate(typeof(NewTaskPage));
            SettingsPageFrame.Navigate(typeof(SettingsPage));
            MainSplitView.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, IsPaneOpenPropertyChanged);
            MainSplitView.SizeChanged += (sender, e) => UpdatePaneWidth();
        }

        private void IsPaneOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            UpdatePaneWidth();
        }

        /// <summary>
        /// Updates the pane width of the main split view
        /// </summary>
        private void UpdatePaneWidth()
        {
            if (MainSplitView.IsPaneOpen)
                MainSplitView.OpenPaneLength = CalculatePaneWidth(MainSplitView.ActualWidth);
            else
                MainSplitView.OpenPaneLength = 0;
        }

        private double CalculatePaneWidth(double actualWidth) => actualWidth * PANE_WIDTH_PERCENTAGE;

        private void ClosePaneButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (NavPageViewModel)DataContext;

            var newTaskPage = (MainSplitView.Pane as Frame)
                .Content as NewTaskPage;
            var newTaskVm = newTaskPage.DataContext as NewTaskPageViewModel;

            if (vm.IsPaneOpen
                && newTaskVm.AppSettings.AskBeforeDiscardChanges
                && newTaskVm.ChangesWereMade())
            {
                newTaskVm.ClosePaneCommand.Execute(null);
                return;
            }
            vm.ClosePaneCommand.Execute(null);
        }
    }
}
