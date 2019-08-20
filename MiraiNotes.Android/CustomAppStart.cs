using Android.OS;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Core.Enums;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MiraiNotes.Android
{
    public class CustomAppStart : MvxAppStart
    {
        private readonly ILogger _logger;
        private readonly IAppSettingsService _appSettings;
        private readonly ISyncService _syncService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;

        public CustomAppStart(
            IMvxApplication application,
            IMvxNavigationService navigationService,
            ILogger logger,
            IAppSettingsService appSettings,
            ISyncService syncService,
            IUserCredentialService userCredentialService,
            IGoogleApiService googleAuthService,
            IMiraiNotesDataService dataService,
            IDialogService dialogService) : base(application, navigationService)
        {
            _logger = logger;
            _appSettings = appSettings;
            _syncService = syncService;
            _userCredentialService = userCredentialService;
            _dataService = dataService;
            _dialogService = dialogService;
        }

        protected override async Task NavigateToFirstViewModel(object hint = null)
        {
            var response = await _dataService
                .UserService
                .GetCurrentActiveUserAsync();


            var loggedUsername = _userCredentialService.GetCurrentLoggedUsername();

            var isUserLoggedIn = loggedUsername != _userCredentialService.DefaultUsername &&
                                 !string.IsNullOrEmpty(loggedUsername);

            if (!response.Succeed || isUserLoggedIn && response.Result is null)
            {
                var errorMsg = string.IsNullOrEmpty(response.Message)
                    ? $"Did you uninstall the app without signing out ?{System.Environment.NewLine}I will properly log you out now..."
                    : response.Message;

                _userCredentialService.DeleteUserCredential(
                    ResourceType.ALL,
                    _userCredentialService.DefaultUsername);

                _logger.Warning(
                    $"{nameof(NavigateToFirstViewModel)}: Couldnt get a user in the db = {response.Succeed} " +
                    $"or isUserLoggedIn and no user exists in db. {errorMsg}");

                await NavigationService.Navigate<LoginViewModel>();
                return;
            }


            if (isUserLoggedIn && _appSettings.AskForPasswordWhenAppStarts)
            {
                _dialogService.ShowLoginDialog(async password =>
                {
                    var result = await PasswordMatches(password);
                    if (result)
                    {
                        await NavigationService.Navigate<MainViewModel>();
                    }
                    else
                    {
                        await NavigationService.Navigate<LoginViewModel>();
                    }
                });
            }
            else if (isUserLoggedIn)
            {
                _dialogService.ShowSucceedToast("navegando...");
                await NavigationService.Navigate<MainViewModel>();
            }
            else
            {
                await NavigationService.Navigate<LoginViewModel>();
            }
        }

        private async Task<bool> PasswordMatches(string pass)
        {
            var response = await _dataService.UserService.GetCurrentActiveUserAsync();
            string currentPassword = _userCredentialService.GetUserCredential(
                ResourceType.SETTINGS_PASSWORD_RESOURCE,
                response.Result.Email);

            return currentPassword == pass;
            //            IsErrorVisible = true;
        }
    }
}