using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Messages;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;

namespace MiraiNotes.Android.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly ILogger _logger;
        private readonly IAppSettingsService _appSettings;
        private readonly ISyncService _syncService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;

        private bool _showLoading;
        private bool _showLoginButton = true;

        public bool ShowLoading
        {
            get => _showLoading;
            set
            {
                SetProperty(ref _showLoading, value);
                ShowLoginButton = !value;
            }
        }

        public bool ShowLoginButton
        {
            get => _showLoginButton;
            set => SetProperty(ref _showLoginButton, value);
        }


        public IMvxCommand LoginCommand { get; private set; }
        public IMvxCommand OnAuthCodeGrantedCommand { get; private set; }
        public IMvxCommand InitViewCommand { get; private set; }

        public LoginViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            ILogger logger,
            IAppSettingsService appSettings,
            ISyncService syncService,
            IUserCredentialService userCredentialService,
            IGoogleApiService googleAuthService,
            IMiraiNotesDataService dataService,
            IDialogService dialogService)
            : base(textProvider, messenger)
        {
            _navigationService = navigationService;
            _logger = logger;
            _appSettings = appSettings;
            _syncService = syncService;
            _userCredentialService = userCredentialService;
            _googleApiService = googleAuthService;
            _dataService = dataService;
            _dialogService = dialogService;
            SetCommands();
        }

        public void SetCommands()
        {
            LoginCommand = new MvxCommand(OnLoginRequest);
            OnAuthCodeGrantedCommand = new MvxAsyncCommand<string>(OnCodeGranted);
            InitViewCommand = new MvxAsyncCommand(InitView);
        }

        public void OnLoginRequest()
        {
            ShowLoading = true;
            var url = _googleApiService.GetAuthorizationUrl();
            Messenger.Publish(new LoginRequestMsg(this, url));
        }

        public void OnLoginCanceled()
        {
            ShowLoading = false;
        }

        public async Task OnCodeGranted(string approvalCode)
        {
            ShowLoading = true;

            try
            {
                var response = await _googleApiService.GetAccessTokenAsync(approvalCode);

                if (!response.Succeed)
                {
                    _logger.Error($"{nameof(OnCodeGranted)}: The token response failed. {response.Message}");
                    throw new Exception(response.Message);
                }

                //We temporaly save and asociate the token to a default user 
                //before doing any other network requst..
                _userCredentialService.DeleteUserCredential(
                    ResourceType.ALL,
                    _userCredentialService.DefaultUsername);

                _userCredentialService.SaveUserCredential(
                    ResourceType.LOGGED_USER_RESOURCE,
                    _userCredentialService.DefaultUsername,
                    _userCredentialService.DefaultUsername);

                _userCredentialService.SaveUserCredential(
                    ResourceType.REFRESH_TOKEN_RESOURCE,
                    _userCredentialService.DefaultUsername,
                    response.Result.RefreshToken);

                _userCredentialService.SaveUserCredential(
                    ResourceType.TOKEN_RESOURCE,
                    _userCredentialService.DefaultUsername,
                    response.Result.AccessToken);

                var isSignedIn = await SignIn();
                if (!isSignedIn)
                {
                    await _dataService
                        .UserService
                        .ChangeCurrentUserStatus(false);

                    _userCredentialService.DeleteUserCredential(
                        ResourceType.ALL,
                        _userCredentialService.DefaultUsername);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"{nameof(OnCodeGranted)}: An unknown error occurred while signing in the app");
                _userCredentialService.DeleteUserCredential(
                    ResourceType.ALL,
                    _userCredentialService.DefaultUsername);
            }
            ShowLoading = false;
        }

        private async Task<bool> SignIn()
        {
            _logger.Information(
                $"{nameof(SignIn)}: Sign in the app started. Trying to get the user info from google");

            var userResponse = await _googleApiService.GetUser();
            if (!userResponse.Succeed)
            {
                _dialogService.ShowErrorToast("User info not found");
                return false;
            }

            var user = userResponse.Result;

            var response = await _dataService
                .UserService
                .ExistsAsync(u => u.GoogleUserID == user.ID);

            var now = DateTimeOffset.UtcNow;
            ResponseDto<GoogleUser> userSaved;
            if (!response.Result)
            {
                _logger.Information($"{nameof(SignIn)}: User doesnt exist in db. Creating a new one...");
                userSaved = await _dataService.UserService.AddAsync(new GoogleUser
                {
                    Email = user.Email,
                    Fullname = user.FullName,
                    PictureUrl = user.ImageUrl,
                    GoogleUserID = user.ID,
                    IsActive = true,
                    CreatedAt = now
                });
            }
            else
            {
                _logger.Information($"{nameof(SignIn)}: User exist in db. Updating it...");
                var userInDbResponse = await _dataService
                    .UserService
                    .FirstOrDefaultAsNoTrackingAsync(u => u.GoogleUserID == user.ID);

                if (!userInDbResponse.Succeed)
                {
                    _dialogService.ShowErrorToast(userInDbResponse.Message);
                    return false;
                }

                userInDbResponse.Result.Fullname = user.FullName;
                userInDbResponse.Result.Email = user.Email;
                userInDbResponse.Result.IsActive = true;
                userInDbResponse.Result.PictureUrl = user.ImageUrl;
                userInDbResponse.Result.UpdatedAt = now;

                userSaved = await _dataService
                    .UserService
                    .UpdateAsync(userInDbResponse.Result);
            }

            if (!userSaved.Succeed)
            {
                _dialogService.ShowErrorToast(
                    $"The user could not be saved / updated into the db. Error = {userSaved.Message}");
                return false;
            }

            _logger.Information($"{nameof(SignIn)}: Trying to get all the task lists that are remote...");
            var syncResult = await _syncService.SyncDownTaskListsAsync(false);

            if (!syncResult.Succeed)
            {
                _dialogService.ShowErrorToast(syncResult.Message);
                return false;
            }

            _logger.Information($"{nameof(SignIn)}: Trying to get all the tasks that are remote...");
            syncResult = await _syncService.SyncDownTasksAsync(false);

            if (!syncResult.Succeed)
            {
                _dialogService.ShowErrorToast(syncResult.Message);
                return false;
            }

            _userCredentialService.UpdateUserCredential(
                ResourceType.LOGGED_USER_RESOURCE,
                _userCredentialService.DefaultUsername,
                false,
                userSaved.Result.Email);
            _userCredentialService.UpdateUserCredential(
                ResourceType.REFRESH_TOKEN_RESOURCE,
                _userCredentialService.DefaultUsername,
                true,
                userSaved.Result.Email);
            _userCredentialService.UpdateUserCredential(
                ResourceType.TOKEN_RESOURCE,
                _userCredentialService.DefaultUsername,
                true,
                userSaved.Result.Email);

            await MiscellaneousUtils.DownloadProfileImage(user.ImageUrl, user.ID);

            //if you came this far, that means everything is ok!
            await _navigationService.Close(this);
            await _navigationService.Navigate<MainViewModel>();
            return true;
        }

        public async Task InitView()
        {
            ShowLoading = true;
            var response = await _dataService
                .UserService
                .GetCurrentActiveUserAsync();


            var loggedUsername = _userCredentialService.GetCurrentLoggedUsername();

            var isUserLoggedIn = loggedUsername != _userCredentialService.DefaultUsername &&
                                 !string.IsNullOrEmpty(loggedUsername);

            if (!response.Succeed || isUserLoggedIn && response.Result is null)
            {
                var errorMsg = string.IsNullOrEmpty(response.Message)
                    ? $"Did you uninstall the app without signing out ?{Environment.NewLine}I will properly log you out now..."
                    : response.Message;
                ShowLoading = false;
                _dialogService.ShowWarningToast(
                    $"Couldn't retrieve the current logged user.{Environment.NewLine}Error = {errorMsg}");

                _userCredentialService.DeleteUserCredential(
                    ResourceType.ALL,
                    _userCredentialService.DefaultUsername);

                _logger.Warning(
                    $"{nameof(InitView)}: Couldnt get a user in the db = {response.Succeed} " +
                    $"or isUserLoggedIn and no user exists in db. {errorMsg}");
                return;
            }

            if (isUserLoggedIn && _appSettings.AskForPasswordWhenAppStarts)
            {
                ShowLoading = false;
                _dialogService.ShowLoginDialog(async password =>
                {
                    var passwordMatches = await PasswordMatches(password);
                    if (passwordMatches)
                    {
                        await _navigationService.Close(this);
                        await _navigationService.Navigate<MainViewModel>();
                    }
                });
            }
            else if (isUserLoggedIn)
            {
                await _navigationService.Close(this);
                await _navigationService.Navigate<MainViewModel>();
            }
            else
            {
                ShowLoading = false;
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