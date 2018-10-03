using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.Data.Models;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.Shared.Models;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Security.Authentication.Web;

namespace MiraiNotes.UWP.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        #region Members
        private readonly ICustomDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IGoogleUserService _googleUserService;

        private readonly IMiraiNotesDataService _dataService;
        private readonly INetworkService _networkService;
        private readonly ISyncService _syncService;
        private readonly IApplicationSettingsService _appSettings;

        private bool _showLoading;
        private bool _showLoginButton;
        #endregion

        #region Properties
        public bool ShowLoading
        {
            get { return _showLoading; }
            set { Set(ref _showLoading, value); }
        }

        public bool ShowLoginButton
        {
            get { return _showLoginButton; }
            set { Set(ref _showLoginButton, value); }
        }
        #endregion

        #region Commands
        public ICommand LoginCommand { get; set; }

        public ICommand LoadedCommand { get; set; }
        #endregion

        #region Constructors
        public LoginPageViewModel(
            ICustomDialogService dialogService,
            INavigationService navigationService,
            IUserCredentialService userCredentialService,
            IGoogleAuthService googleAuthService,
            IGoogleUserService googleUserService,
            IMiraiNotesDataService dataService,
            INetworkService networkService,
            ISyncService syncService,
            IApplicationSettingsService appSettings)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userCredentialService = userCredentialService;
            _googleAuthService = googleAuthService;
            _googleUserService = googleUserService;
            _dataService = dataService;
            _networkService = networkService;
            _syncService = syncService;
            _appSettings = appSettings;

            SetCommands();
        }
        #endregion

        #region Methods    

        private void SetCommands()
        {
            LoadedCommand = new RelayCommand
                (async () => await InitViewAsync());
            LoginCommand = new RelayCommand(SignInWithGoogleAsync);

        }

        public async Task InitViewAsync()
        {
            ShowLoading = true;
            var currentUserResponse = await _dataService
                .UserService
                .GetCurrentActiveUserAsync();
            string loggedUsername = _userCredentialService.GetCurrentLoggedUsername();

            bool isUserLoggedIn = loggedUsername != _userCredentialService.DefaultUsername &&
                !string.IsNullOrEmpty(loggedUsername);

            if (!currentUserResponse.Succeed || isUserLoggedIn && currentUserResponse.Result is null)
            {
                ShowLoading = false;
                ShowLoginButton = true;
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't retrieve the current logged user. Error = {currentUserResponse.Message}");
                return;
            }
            ShowLoading = false;

            if (isUserLoggedIn && _appSettings.AskForPasswordWhenAppStarts)
            {
                var result = await _dialogService.ShowCustomDialog(CustomDialogType.LOGIN_PASSWORD_DIALOG);
                if (result)
                    _navigationService.NavigateTo(ViewModelLocator.HOME_PAGE);
                else
                    ShowLoginButton = true;
            }
            else if (isUserLoggedIn)
                _navigationService.NavigateTo(ViewModelLocator.HOME_PAGE);
            else
                ShowLoginButton = true;
        }

        public async void SignInWithGoogleAsync()
        {
            if (!_networkService.IsInternetAvailable())
            {
                await _dialogService.ShowMessageDialogAsync("Error", "Network is not available");
                return;
            }

            ShowLoading = true;
            ShowLoginButton = false;
            Uri requestUri = new Uri(_googleAuthService.GetAuthorizationUrl());
            Uri callbackUri = new Uri(_googleAuthService.ApprovalUrl);
            try
            {
                var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, requestUri, callbackUri);
                switch (result.ResponseStatus)
                {
                    case WebAuthenticationStatus.Success:
                        var queryParams = result.ResponseData
                            .Split('&')
                            .ToDictionary(c => c.Split('=')[0], c => Uri.UnescapeDataString(c.Split('=')[1]));

                        if (queryParams.ContainsKey("error"))
                        {
                            await _dialogService.ShowMessageDialogAsync("Error", $"OAuth authorization error: {queryParams["error"]}");
                            return;
                        }

                        if (!queryParams.ContainsKey("approvalCode"))
                        {
                            await _dialogService.ShowMessageDialogAsync("Error", "Malformed authorization response.");
                            return;
                        }

                        // Gets the Authorization code
                        string approvalCode = queryParams["approvalCode"];
                        var tokenResponse = await _googleAuthService.GetTokenAsync(approvalCode);
                        if (tokenResponse == null)
                        {
                            await _dialogService.ShowMessageDialogAsync("Something happended...!", "Couldn't get a token");
                            return;
                        }
                        //We save the token before doing any other network requst..
                        _userCredentialService.DeleteUserCredential(
                            PasswordVaultResourceType.ALL, 
                            _userCredentialService.DefaultUsername);

                        _userCredentialService.SaveUserCredential(
                            PasswordVaultResourceType.LOGGED_USER_RESOURCE,
                            _userCredentialService.DefaultUsername,
                            _userCredentialService.DefaultUsername);

                        _userCredentialService.SaveUserCredential(
                            PasswordVaultResourceType.REFRESH_TOKEN_RESOURCE,
                            _userCredentialService.DefaultUsername, 
                            tokenResponse.RefreshToken);

                        _userCredentialService.SaveUserCredential(
                            PasswordVaultResourceType.TOKEN_RESOURCE,
                            _userCredentialService.DefaultUsername,
                            tokenResponse.AccessToken);

                        bool isSignedIn = await SignInAsync();
                        if (!isSignedIn)
                        {
                            await _dataService
                                .UserService
                                .ChangeCurrentUserStatus(false);

                            _userCredentialService.DeleteUserCredential(
                                PasswordVaultResourceType.ALL, 
                                _userCredentialService.DefaultUsername);
                        }
                        break;
                    case WebAuthenticationStatus.UserCancel:
                        break;
                    case WebAuthenticationStatus.ErrorHttp:
                        await _dialogService.ShowMessageDialogAsync("Error", result.ResponseErrorDetail.ToString());
                        break;
                    default:
                        await _dialogService.ShowMessageDialogAsync("Error", result.ResponseData.ToString());
                        break;
                }
            }
            catch (Exception ex)
            {
                _userCredentialService.DeleteUserCredential(
                    PasswordVaultResourceType.ALL,
                    _userCredentialService.DefaultUsername);
                await _dialogService.ShowErrorMessageDialogAsync(ex, "An unknown error occurred");
            }
            finally
            {
                ShowLoading = false;
                ShowLoginButton = true;
            }
        }

        private async Task<bool> SignInAsync()
        {
            bool result = false;
            var user = await _googleUserService.GetUserInfoAsync();
            if (user == null)
            {
                await _dialogService.ShowMessageDialogAsync("Something happended...!", "User info not found");
                return result;
            }

            var response = await _dataService
                .UserService
                .ExistsAsync(u => u.GoogleUserID == user.ID);
            Response<GoogleUser> userSaved;
            if (!response.Result)
            {
                userSaved = await _dataService.UserService.AddAsync(new GoogleUser
                {
                    Email = user.Email,
                    Fullname = user.FullName,
                    PictureUrl = user.ImageUrl,
                    GoogleUserID = user.ID,
                    IsActive = true
                });
            }
            else
            {
                var userInDbResponse = await _dataService
                    .UserService
                    .FirstOrDefaultAsNoTrackingAsync(u => u.GoogleUserID == user.ID);

                if (!userInDbResponse.Succeed)
                {
                    await _dialogService.ShowMessageDialogAsync("Error", userInDbResponse.Message);
                    return result;
                }

                userInDbResponse.Result.Fullname = user.FullName;
                userInDbResponse.Result.Email = user.Email;
                userInDbResponse.Result.IsActive = true;
                userInDbResponse.Result.PictureUrl = user.ImageUrl;

                userSaved = await _dataService
                    .UserService
                    .UpdateAsync(userInDbResponse.Result);
            }

            if (!userSaved.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"The user could not be saved / updated into the db. Error = {userSaved.Message}");
                return result;
            }

            var syncResult = await _syncService.SyncDownTaskListsAsync(false);

            if (!syncResult.Succeed)
            {
                await _dialogService
                    .ShowMessageDialogAsync("Error", syncResult.Message);
                return result;
            }

            syncResult = await _syncService.SyncDownTasksAsync(false);

            if (!syncResult.Succeed)
            {
                await _dialogService
                    .ShowMessageDialogAsync("Error", syncResult.Message);
                return result;
            }

            _userCredentialService.UpdateUserCredential(
                PasswordVaultResourceType.LOGGED_USER_RESOURCE,
                _userCredentialService.DefaultUsername,
                false,
                userSaved.Result.Email);
            _userCredentialService.UpdateUserCredential(
                PasswordVaultResourceType.REFRESH_TOKEN_RESOURCE,
                _userCredentialService.DefaultUsername,
                true,
                userSaved.Result.Email);
            _userCredentialService.UpdateUserCredential(
                PasswordVaultResourceType.TOKEN_RESOURCE,
                _userCredentialService.DefaultUsername,
                true,
                userSaved.Result.Email);

            await _googleUserService.RemoveProfileImage();

            await _googleUserService.DownloadProfileImage(user.ImageUrl);

            _navigationService.NavigateTo(ViewModelLocator.HOME_PAGE);
            return !result;
        }
        #endregion
    }
}
