using GalaSoft.MvvmLight.Messaging;
using MiraiNotes.UWP.Pages;
using MiraiNotes.UWP.Pages.Settings;
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
    }
}
