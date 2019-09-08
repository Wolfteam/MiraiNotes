using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsHomeViewModel : BaseViewModel
    {
        #region Members
        private readonly IMvxNavigationService _navigationService;
        private readonly IDialogService _dialogService;

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

        public List<SettingsPageItem> SettingsPages { get; } = new List<SettingsPageItem>
        {
            new SettingsPageItem
            {
                Content = "Default sort order, themes, etc.",
                Header = "General",
                PageType = SettingsPageType.GENERAL
            },
            new SettingsPageItem
            {
                Content = "Background tasks intervals.",
                Header = "Synchronization",
                PageType = SettingsPageType.SYNCHRONIZATION
            },
            new SettingsPageItem
            {
                Content = "Toast notifications.",
                Header = "Notifications",
                PageType = SettingsPageType.NOTIFICATIONS
            },
            new SettingsPageItem
            {
                Content = "Github repo, contact information, donations.",
                Header = "About",
                PageType = SettingsPageType.ABOUT
            }
        };

        //public bool IsBackButtonVisible
        //{
        //    get => _isBackButtonVisible;
        //    set => Set(ref _isBackButtonVisible, value);
        //}



        #endregion

        public IMvxAsyncCommand<SettingsPageItem> SettingItemSelectedCommand { get; private set; }


        public SettingsHomeViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            IDialogService dialogService)
            : base(textProvider, messenger, appSettings)
        {
            _navigationService = navigationService;
            _dialogService = dialogService;

            SetCommands();
        }

        private void SetCommands()
        {
            SettingItemSelectedCommand = new MvxAsyncCommand<SettingsPageItem>(NavigateTo);
        }

        private async Task NavigateTo(SettingsPageItem page)
        {
            switch (page.PageType)
            {
                case SettingsPageType.GENERAL:
                    await _navigationService.Navigate<SettingsGeneralViewModel>();
                    break;
                case SettingsPageType.SYNCHRONIZATION:
                    await _navigationService.Navigate<SettingsSyncViewModel>();
                    break;
                case SettingsPageType.NOTIFICATIONS:
                    await _navigationService.Navigate<SettingsNotificationsViewModel>();
                    break;
                case SettingsPageType.ABOUT:
                    await _navigationService.Navigate<SettingsAboutViewModel>();
                    break;
                case SettingsPageType.HOME:
                default:
                    throw new ArgumentOutOfRangeException(nameof(page.PageType), page, "Invalid settings page");
            }
        }
    }
}