using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Dto.Google.Responses;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Utils;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiraiNotes.UWP.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        #region Members
        private readonly ILogger _logger;
        private readonly ICustomDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleApiService _googleApiService;

        private readonly IMiraiNotesDataService _dataService;
        private readonly INetworkService _networkService;
        private readonly ISyncService _syncService;
        private readonly IAppSettingsService _appSettings;

        private bool _showLoading;
        private bool _showLoginButton;
        #endregion

        #region Properties
        public bool ShowLoading
        {
            get => _showLoading;
            set => Set(ref _showLoading, value);
        }

        public bool ShowLoginButton
        {
            get => _showLoginButton;
            set => Set(ref _showLoginButton, value);
        }
        #endregion

        #region Commands
        public ICommand LoginCommand { get; set; }

        public ICommand LoadedCommand { get; set; }
        #endregion

        #region Constructors
        public LoginPageViewModel(
            ILogger logger,
            ICustomDialogService dialogService,
            INavigationService navigationService,
            IUserCredentialService userCredentialService,
            IGoogleApiService googleApiService,
            IMiraiNotesDataService dataService,
            INetworkService networkService,
            ISyncService syncService,
            IAppSettingsService appSettings)
        {
            _logger = logger.ForContext<LoginPageViewModel>();
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userCredentialService = userCredentialService;
            _googleApiService = googleApiService;
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
            var response = await _dataService
                .UserService
                .GetCurrentActiveUserAsync();
            var loggedUsername = _userCredentialService.GetCurrentLoggedUsername();

            var isUserLoggedIn = loggedUsername != _userCredentialService.DefaultUsername &&
                                 !string.IsNullOrEmpty(loggedUsername);

            if (!response.Succeed || isUserLoggedIn && response.Result is null)
            {
                var errorMsg = string.IsNullOrEmpty(response.Message)
                    ? $"Did you unninstall the app without signing out ?{Environment.NewLine}I will properly log you out now..."
                    : response.Message;
                ShowLoading = false;
                ShowLoginButton = true;
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"Coudln't retrieve the current logged user.{Environment.NewLine}Error = {errorMsg}");
                _userCredentialService.DeleteUserCredential(
                    ResourceType.ALL,
                    _userCredentialService.DefaultUsername);

                _logger.Warning($"{nameof(InitViewAsync)}: Couldnt get a user in the db = {response.Succeed} " +
                                $"or isUserLoggedIn and no user exists in db. {errorMsg}");
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
            {
                _navigationService.NavigateTo(ViewModelLocator.HOME_PAGE);
            }
            else
            {
                ShowLoginButton = true;
            }
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

            _logger.Information($"{nameof(SignInWithGoogleAsync)}: Trying to sign in with google...");
            var response = await _googleApiService.SignInWithGoogle();
            await OnGoogleSignInResponse(response);

            ShowLoading = false;
            ShowLoginButton = true;
        }

        private async Task OnGoogleSignInResponse(ResponseDto<TokenResponseDto> response)
        {
            try
            {
                if (!response.Succeed)
                {
                    //user canceled auth..
                    if (string.IsNullOrEmpty(response.Message))
                        return;

                    _logger.Error($"{nameof(OnGoogleSignInResponse)}: The token response failed. {response.Message}");
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

                var isSignedIn = await SignInAsync();
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
                    $"{nameof(OnGoogleSignInResponse)}: An unknown error occurred while signing in the app");
                _userCredentialService.DeleteUserCredential(
                    ResourceType.ALL,
                    _userCredentialService.DefaultUsername);
                await _dialogService.ShowErrorMessageDialogAsync(ex, "An unknown error occurred");
            }
        }

        private async Task<bool> SignInAsync()
        {
            _logger.Information(
                $"{nameof(SignInAsync)}: Sign in the app started. Trying to get the user info from google");
            var result = false;
            var userResponse = await _googleApiService.GetUser();
            if (!userResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync("Something happended...!", "User info not found");
                return result;
            }
            var user = userResponse.Result;
            var response = await _dataService
                .UserService
                .ExistsAsync(u => u.GoogleUserID == user.ID);

            var now = DateTimeOffset.UtcNow;
            ResponseDto<GoogleUser> userSaved;
            if (!response.Succeed)
                throw new Exception($"Couldnt check if userId = {user.ID} exists on db");

            if (!response.Result)
            {
                _logger.Information($"{nameof(SignInAsync)}: User doesnt exist in db. Creating a new one...");
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
                _logger.Information($"{nameof(SignInAsync)}: User exist in db. Updating it...");
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
                userInDbResponse.Result.UpdatedAt = now;

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

            _logger.Information($"{nameof(SignInAsync)}: Trying to get all the task lists that are remote...");
            var syncResult = await _syncService.SyncDownTaskListsAsync(false);

            if (!syncResult.Succeed)
            {
                await _dialogService
                    .ShowMessageDialogAsync("Error", syncResult.Message);
                return result;
            }

            _logger.Information($"{nameof(SignInAsync)}: Trying to get all the tasks that are remote...");
            syncResult = await _syncService.SyncDownTasksAsync(false);

            if (!syncResult.Succeed)
            {
                await _dialogService
                    .ShowMessageDialogAsync("Error", syncResult.Message);
                return result;
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
            _navigationService.NavigateTo(ViewModelLocator.HOME_PAGE);
            return !result;
        }

        #endregion
    }
}