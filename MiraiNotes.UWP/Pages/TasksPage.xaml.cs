using MiraiNotes.UWP.Controls;
using MiraiNotes.UWP.Utils;
using MiraiNotes.UWP.ViewModels;
using System;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MiraiNotes.UWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TasksPage : Page
    {
        private int _lastSelectedIndex;
        private bool _selectionInProgress;

        public TasksPage()
        {
            this.InitializeComponent();
            var vm = (TasksPageViewModel)DataContext;
            vm.InAppNotificationRequest += ShowInAppNotification;
        }

        private void TaskList_ComboBox_DropDownOpened(object sender, object e)
        {
            var vm = (TasksPageViewModel)DataContext;
            vm.MoveComboBoxOpenedCommand.Execute(null);
        }

        private void SubTasks_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = (TasksPageViewModel)DataContext;
            var task = (sender as ListView).SelectedItem as TaskItemViewModel;
            vm.SubTaskSelectedItemCommand.Execute(task);
        }

        private void ShowInAppNotification(string message)
        {
            Task_InAppNotification.Show(message);
        }

        private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listView = sender as TasksListView;
            var vm = (TasksPageViewModel)DataContext;

            if (_selectionInProgress)
                return;

            try
            {
                //if nothing is selected
                if (listView.SelectedIndex == -1)
                {
                    vm.TaskListViewSelectedItemCommand.Execute(null);
                    return;
                }

                _selectionInProgress = true;

                var navVm = Frame.DataContext as NavPageViewModel;

                var parent = ((Frame.Parent as Border).Parent as Grid).Parent as Grid;
                var newTaskPage = MiscellaneousUtils.FindControl<Grid>(parent, "MainSplitViewPane");
                var newTaskVm = newTaskPage.DataContext as NewTaskPageViewModel;

                if (navVm.IsPaneOpen
                    && newTaskVm.AppSettings.AskBeforeDiscardChanges
                    && newTaskVm.ChangesWereMade())
                {
                    vm.DesiredTaskIndex = listView.SelectedIndex;
                    listView.SelectedIndex = _lastSelectedIndex;
                    newTaskVm.ClosePaneCommand.Execute(null);
                    return;
                }

                _lastSelectedIndex = listView.SelectedIndex;

                vm.TaskListViewSelectedItemCommand.Execute(listView.SelectedItem);
            }
            catch (Exception)
            {

            }
            finally
            {
                _selectionInProgress = false;
            }

        }
    }
}
