using GalaSoft.MvvmLight.Messaging;
using MiraiNotes.UWP.Pages;
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
            MainSplitView.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, IsPaneOpenPropertyChanged);
            MainSplitView.SizeChanged += (sender, e) => MainSplitViewSizeChanged();
        }

        /// <summary>
        /// Updates the pane view width of the main split view
        /// </summary>
        private void MainSplitViewSizeChanged()
        {
            //TODO: When the hamburger is open and you minimize the screen, a bug appears
            if (!MainSplitView.IsPaneOpen)
                return;
            
            if (PaneFrame.ActualWidth == 0 || ContentFrame.ActualWidth == 0)
            {
                MainSplitView.OpenPaneLength = CalculatePaneWidth(MainSplitView.ActualWidth);
                return;
            }

            if (PaneFrame.ActualWidth <= ContentFrame.ActualWidth * PANE_WIDTH_PERCENTAGE)
                MainSplitView.OpenPaneLength = CalculatePaneWidth(ContentFrame.ActualWidth);
            else if (PaneFrame.ActualWidth * PANE_WIDTH_PERCENTAGE > ContentFrame.ActualWidth)
                MainSplitView.OpenPaneLength = CalculatePaneWidth(ContentFrame.ActualWidth);
        }

        private void IsPaneOpenPropertyChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (MainSplitView.IsPaneOpen)
                MainSplitView.OpenPaneLength = CalculatePaneWidth(ContentFrame.ActualWidth);
            else
                MainSplitView.OpenPaneLength = CalculatePaneWidth(0);
        }

        private double CalculatePaneWidth(double actualWidth) => actualWidth * PANE_WIDTH_PERCENTAGE;
    }
}
