using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MiraiNotes.UWP.Pages.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsGeneralPage : Page
    {
        public object Colors { get; set; }
        public SettingsGeneralPage()
        {
            this.InitializeComponent();
            Colors = typeof(Colors).GetRuntimeProperties().Select(x => new
            {
                Color = (Color)x.GetValue(null)
            });
        }

        private void gridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lista = new List<string>
            {
                "SystemControlBackgroundAccentBrush", "SystemControlDisabledAccentBrush",
                "SystemControlForegroundAccentBrush", "SystemControlHighlightAccentBrush",
                "SystemControlHighlightAltAccentBrush", "SystemControlHighlightAltListAccentHighBrush",
                "SystemControlHighlightAltListAccentLowBrush", "SystemControlHighlightAltListAccentMediumBrush",
                "SystemControlHighlightListAccentHighBrush", "SystemControlHighlightListAccentLowBrush",
                "SystemControlHighlightListAccentMediumBrush", "SystemControlHighlightListAccentMediumBrush",
                "SystemControlHyperlinkTextBrush", "ContentDialogBorderThemeBrush",
                "JumpListDefaultEnabledBackground", "SystemControlHighlightAltBaseHighBrush"
            };
            //dynamic g = (sender as GridView).SelectedItem;
            //var selectedColor = (Color)g.Color;
            //ApplicationData.Current.LocalSettings.Values["AccentColor"] = selectedColor.ToString();

            //var color = Application.Current.Resources["SystemAccentColor"];
            //var tb = ApplicationView.GetForCurrentView().TitleBar;
            //tb.BackgroundColor =
            //    tb.ButtonBackgroundColor =
            //        tb.InactiveBackgroundColor =
            //            tb.ButtonInactiveBackgroundColor = selectedColor;


            //Application.Current.Resources["SystemAccentColor"] = selectedColor;

            //foreach (string key in lista)
            //{
            //    (Application.Current.Resources[key] as SolidColorBrush).Color = (Color)g.Color;
            //}
        }
    }
}
