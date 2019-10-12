using Microsoft.AppCenter.Crashes;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.ViewModels.Dialogs;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Plugin.Fingerprint.Abstractions;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        #region Members
        private readonly ISyncService _syncService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;
        private readonly IFingerprint _fingerprintService;
        private bool _showLoading;
        private bool _showLoginButton = true;

        private readonly MvxInteraction<string> _loginRequest = new MvxInteraction<string>();
        #endregion

        #region Interactors
        public IMvxInteraction<string> LoginRequest
            => _loginRequest;
        #endregion

        #region Properties
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

        #endregion

        #region Commands
        public IMvxCommand LoginCommand { get; private set; }
        public IMvxCommand OnAuthCodeGrantedCommand { get; private set; }
        public IMvxCommand InitViewCommand { get; private set; }
        #endregion

        public LoginViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            ILogger logger,
            IAppSettingsService appSettings,
            ISyncService syncService,
            IUserCredentialService userCredentialService,
            IGoogleApiService googleAuthService,
            IMiraiNotesDataService dataService,
            IDialogService dialogService,
            IFingerprint fingerprint,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<LoginViewModel>(), navigationService, appSettings, telemetryService)
        {
            _syncService = syncService;
            _userCredentialService = userCredentialService;
            _googleApiService = googleAuthService;
            _dataService = dataService;
            _dialogService = dialogService;
            _fingerprintService = fingerprint;

            TextProvider.SetLanguage(AppSettings.AppLanguage, false);
        }

        public override void SetCommands()
        {
            base.SetCommands();
            LoginCommand = new MvxCommand(OnLoginRequest);
            OnAuthCodeGrantedCommand = new MvxAsyncCommand<string>(OnCodeGranted);
            InitViewCommand = new MvxAsyncCommand(InitView);
        }

        public void OnLoginRequest()
        {
            ShowLoading = true;
            var url = _googleApiService.GetAuthorizationUrl();
            _loginRequest.Raise(url);
        }

        public async Task OnCodeGranted(string approvalCode)
        {
            ShowLoading = true;

            try
            {
                var response = await _googleApiService.GetAccessTokenAsync(approvalCode);

                if (!response.Succeed)
                {
                    Logger.Error($"{nameof(OnCodeGranted)}: The token response failed. {response.Message}");
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
                Logger.Error(ex,
                    $"{nameof(OnCodeGranted)}: An unknown error occurred while signing in the app");
                _userCredentialService.DeleteUserCredential(
                    ResourceType.ALL,
                    _userCredentialService.DefaultUsername);
            }
            ShowLoading = false;
        }

        private async Task<bool> SignIn()
        {
            Logger.Information(
                $"{nameof(SignIn)}: Trying to get the user info from google");

            var userResponse = await _googleApiService.GetUser();
            if (!userResponse.Succeed)
            {
                Logger.Error($"{nameof(SignIn)}: Couldnt get the google user. Error = {userResponse.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
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
                Logger.Information($"{nameof(SignIn)}: User doesnt exist in db. Creating a new one...");
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
                Logger.Information($"{nameof(SignIn)}: User exist in db. Updating it...");
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
                Logger.Error($"The user could not be saved / updated into the db. Error = {userSaved.Message}");
                _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                return false;
            }

            Logger.Information($"{nameof(SignIn)}: Trying to get all the task lists that are remote...");
            var syncResult = await _syncService.SyncDownTaskListsAsync(false);

            if (!syncResult.Succeed)
            {
                Logger.Error($"{nameof(SignIn)}: Couldnt sync down tasklists. Error = {syncResult.Message}");
                _dialogService.ShowErrorToast(GetText("SyncUnknownError"));
                return false;
            }

            Logger.Information($"{nameof(SignIn)}: Trying to get all the tasks that are remote...");
            syncResult = await _syncService.SyncDownTasksAsync(false);

            if (!syncResult.Succeed)
            {
                Logger.Error($"{nameof(SignIn)}: Couldnt sync down tasks. Error = {syncResult.Message}");
                _dialogService.ShowErrorToast(GetText("SyncUnknownError"));
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
            await NavigationService.Close(this);
            await NavigationService.Navigate<MainViewModel>();
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
                ShowLoading = false;

                var errorMsg = string.IsNullOrEmpty(response.Message)
                    ? GetText("UnnistalledAppWithoutSigningOut")
                    : response.Message;
                Logger.Warning(
                    $"{nameof(InitView)}:  Couldnt get a user in the db = {response.Succeed}." +
                    "Or isUserLoggedIn and no user exists in db" +
                    $"{Environment.NewLine}Error = {errorMsg}");

                _dialogService.ShowWarningToast(GetText("UnnistalledAppWithoutSigningOut"));

                _userCredentialService.DeleteUserCredential(
                    ResourceType.ALL,
                    _userCredentialService.DefaultUsername);
                return;
            }

            var fingerprintAvailability = await _fingerprintService.GetAvailabilityAsync();

            bool isAuthenticated = false;

            if (isUserLoggedIn &&
                fingerprintAvailability == FingerprintAvailability.Available &&
                AppSettings.AskForFingerPrintWhenAppStarts)
            {
                isAuthenticated = await LoginWithFingerPrint();
            }
            else if (isUserLoggedIn && AppSettings.AskForPasswordWhenAppStarts)
            {
                isAuthenticated = await LoginWithPassword();
            }
            else if (isUserLoggedIn)
            {
                isAuthenticated = true;
                await GoToMainPage();
            }

            if (!isAuthenticated)
                ShowLoading = false;
        }

        private async Task GoToMainPage()
        {
            await NavigationService.Close(this);
            await NavigationService.Navigate<MainViewModel>();
        }

        private async Task<bool> LoginWithFingerPrint()
        {
            var isAuthenticated = false;
            while (!isAuthenticated)
            {
                var authResult = await _fingerprintService.AuthenticateAsync(GetText("FingerprintAuthMsg"));
                if (authResult.Status == FingerprintAuthenticationResultStatus.Succeeded &&
                    authResult.Authenticated)
                {
                    isAuthenticated = true;
                }
                else if (authResult.Status == FingerprintAuthenticationResultStatus.Canceled)
                {
                    break;
                }
                else
                {
                    _dialogService.ShowErrorToast(GetText("FingerprintAuthFailed"));
                }
            }

            if (isAuthenticated)
                await GoToMainPage();

            return isAuthenticated;
        }

        private async Task<bool> LoginWithPassword()
        {
            var passwordMatches = await NavigationService.Navigate<PasswordDialogViewModel, bool, bool>(true);
            if (passwordMatches)
                await GoToMainPage();

            return passwordMatches;
        }
    }
}