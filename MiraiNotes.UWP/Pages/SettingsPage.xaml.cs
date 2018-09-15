using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Pages.Settings;
using MiraiNotes.UWP.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MiraiNotes.UWP.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPageViewModel SettingsViewModel => DataContext as SettingsPageViewModel;

        public SettingsPage()
        {
            this.InitializeComponent();
            SettingsViewModel.NavigationRequest += OnNavigationRequest;
            SettingsPageFrame.Navigate(typeof(SettingsMainPage));
        }

        private void OnNavigationRequest(SettingsPageType settingsPageType)
        {
            Type page;
            SettingsViewModel.IsBackButtonVisible = true;
            switch (settingsPageType)
            {
                case SettingsPageType.HOME:
                    SettingsViewModel.IsBackButtonVisible = false;
                    if (SettingsPageFrame.CanGoBack)
                    {
                        SettingsPageFrame.GoBack();
                        return;
                    }
                    else
                        page = typeof(SettingsMainPage);
                    break;
                case SettingsPageType.GENERAL:
                    page = typeof(SettingsGeneralPage);
                    break;
                case SettingsPageType.ACCOUNT:
                    throw new NotImplementedException("Account settings page is not implemented");
                case SettingsPageType.SYNCHRONIZATION:
                    page = typeof(SettingsSynchronizationPage);
                    break;
                case SettingsPageType.NOTIFICATIONS:
                    page = typeof(SettingsNotificationsPage);
                    break;
                case SettingsPageType.ABOUT:
                    page = typeof(SettingsAboutPage);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(settingsPageType), settingsPageType, "Cannot navigate to the current selected settings page");
            }
            SettingsPageFrame.Navigate(page);
        }

        private void SettingsPageFrame_Navigated(object sender, NavigationEventArgs e)
        {
            (SettingsPageFrame.Content as FrameworkElement).DataContext = SettingsViewModel;
        }
    }
}
