﻿using MiraiNotes.UWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MiraiNotes.UWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TasksPage : Page
    {
        public TasksPage()
        {
            this.InitializeComponent();
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
    }
}
