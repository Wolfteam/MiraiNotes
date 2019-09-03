using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MiraiNotes.UWP.Pages.Settings;
using MiraiNotes.UWP.ViewModels;
using System;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MiraiNotes.UWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPageViewModel ViewModel => DataContext as SettingsPageViewModel;

        public SettingsPage()
        {
            this.InitializeComponent();
            ViewModel.NavigationRequest += OnNavigationRequest;
            OnNavigationRequest(SettingsPageType.HOME);
        }

        private void OnNavigationRequest(SettingsPageType settingsPageType)
        {
            Type page;
            ViewModel.IsBackButtonVisible = true;
            switch (settingsPageType)
            {
                case SettingsPageType.HOME:
                    ViewModel.CurrentPageText = "Settings";
                    ViewModel.IsBackButtonVisible = false;
                    if (SettingsPageFrame.CanGoBack)
                    {
                        SettingsPageFrame.GoBack();
                        return;
                    }
                    else
                        page = typeof(SettingsMainPage);
                    break;
                case SettingsPageType.GENERAL:
                    ViewModel.CurrentPageText = "General";
                    page = typeof(SettingsGeneralPage);
                    break;
                case SettingsPageType.SYNCHRONIZATION:
                    ViewModel.CurrentPageText = "Syncrhonization";
                    page = typeof(SettingsSynchronizationPage);
                    break;
                case SettingsPageType.NOTIFICATIONS:
                    ViewModel.CurrentPageText = "Notifications";
                    page = typeof(SettingsNotificationsPage);
                    break;
                case SettingsPageType.ABOUT:
                    ViewModel.CurrentPageText = "About";
                    page = typeof(SettingsAboutPage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(settingsPageType), settingsPageType, "Cannot navigate to the current selected settings page");
            }
            SettingsPageFrame.Navigate(page);
        }
    }
}
