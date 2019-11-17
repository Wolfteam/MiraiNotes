using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsHomeViewModel : BaseViewModel
    {
        #region Members
        private string _currentPageText;
        private readonly MvxInteraction<SettingsPageType> _onSettingItemSelected = new MvxInteraction<SettingsPageType>();
        #endregion

        #region Properties
        public IMvxInteraction<SettingsPageType> OnSettingItemSelected
            => _onSettingItemSelected;

        public string CurrentPageText
        {
            get => _currentPageText;
            set => SetProperty(ref _currentPageText, value);
        }

        public List<SettingsPageItem> SettingsPages => new List<SettingsPageItem>
        {
            new SettingsPageItem
            {
                Content = GetText("SettingsGeneral"),
                Header =  GetText("General"),
                PageType = SettingsPageType.GENERAL
            },
            new SettingsPageItem
            {
                Content = GetText("SettingsSync"),
                Header = GetText("Synchronization"),
                PageType = SettingsPageType.SYNCHRONIZATION
            },
            new SettingsPageItem
            {
                Content = GetText("SettingsNotif"),
                Header = GetText("Notifications"),
                PageType = SettingsPageType.NOTIFICATIONS
            },
            new SettingsPageItem
            {
                Content = GetText("SettingsAbout"),
                Header = GetText("About"),
                PageType = SettingsPageType.ABOUT
            }
        };
        #endregion

        #region Commands
        public IMvxAsyncCommand<SettingsPageItem> SettingItemSelectedCommand { get; private set; }

        #endregion

        public SettingsHomeViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            ILogger logger,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<SettingsMainViewModel>(), navigationService, appSettings, telemetryService)
        {
        }

        public override void ViewAppeared()
        {
            Messenger.Publish(new SettingsTitleChanged(this, GetText("Settings")));
            base.ViewAppeared();
        }

        public override void SetCommands()
        {
            base.SetCommands();
            SettingItemSelectedCommand = new MvxAsyncCommand<SettingsPageItem>(NavigateTo);
        }

        private async Task NavigateTo(SettingsPageItem page)
        {
            string title;
            Type to;
            switch (page.PageType)
            {
                case SettingsPageType.GENERAL:
                    title = GetText("General");
                    to = typeof(SettingsGeneralViewModel);
                    break;
                case SettingsPageType.SYNCHRONIZATION:
                    title = GetText("Synchronization");
                    to = typeof(SettingsSyncViewModel);
                    break;
                case SettingsPageType.NOTIFICATIONS:
                    title = GetText("Notifications");
                    to = typeof(SettingsNotificationsViewModel);
                    break;
                case SettingsPageType.ABOUT:
                    title = GetText("About");
                    to = typeof(SettingsAboutViewModel);
                    break;
                case SettingsPageType.HOME:
                default:
                    Logger.Warning($"{nameof(NavigateTo)}: Trying to navigate to a page that shoulnt be navigated to. Page = {page.PageType}");
                    throw new ArgumentOutOfRangeException(nameof(page.PageType), page, "Invalid settings page");
            }
            Messenger.Publish(new SettingsTitleChanged(this, title));
            await NavigationService.Navigate(to);
        }
    }
}