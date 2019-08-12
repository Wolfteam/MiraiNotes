using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Messages;
using MiraiNotes.Android.Services;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;

namespace MiraiNotes.Android.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly ILogger _logger;

        private readonly IAppSettingsService _appSettings;

        //        private readonly ISyncService _syncService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IMiraiNotesDataService _dataService;
        private bool _showLoading;

        public bool ShowLoading
        {
            get => _showLoading;
            set => SetProperty(ref _showLoading, value);
        }

        public bool ShowLoginButton
        {
            get => _showLoading;
            set => SetProperty(ref _showLoading, value);
        }


        public ICommand LoginCommand { get; set; }

        public LoginViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            ILogger logger,
            IAppSettingsService appSettings,
            //            ISyncService syncService,
            IUserCredentialService userCredentialService,
            IGoogleApiService googleAuthService,
            IMiraiNotesDataService dataService)
            : base(textProvider, messenger)
        {
            _navigationService = navigationService;
            _logger = logger;
            _appSettings = appSettings;
            //            _syncService = syncService;
            _userCredentialService = userCredentialService;
            _googleApiService = googleAuthService;
            _dataService = dataService;

            SetCommands();

            //CUANDO VUELVES DEL ONRESUME EL CONSTRUCTOR ES VUELTO A LLAMAR POR LO QUE 
            //OBTIENES 2 SUBSCRIBCIONES Y ESO HACE Q SE EJECUTE 2 VECES EL CODIGO DE ABAJP
            SubscriptionTokens.Add(Messenger.Subscribe<AuthCodeGrantedMsg>
                (async msg => await OnCodeGranted(msg.AuthCode)));
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            await InitViewAsync();
        }

        public void SetCommands()
        {
            LoginCommand = new MvxCommand(async () => await OnLoginRequest());
        }

        public async Task OnLoginRequest()
        {
            ShowLoading = true;
            ShowLoginButton = false;
            await Task.Delay(5000);
            await _navigationService.Navigate<MainViewModel>();
            return;
            var url = _googleApiService.GetAuthorizationUrl();
            Messenger.Publish(new LoginRequestMsg(this, url));
        }

        public void OnLoginCanceled()
        {
            ShowLoading = false;
            ShowLoginButton = true;
        }

        public async Task OnCodeGranted(string approvalCode)
        {
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
                    $"{nameof(OnCodeGranted)}: An unknown error occurred while signing in the app");
                _userCredentialService.DeleteUserCredential(
                    ResourceType.ALL,
                    _userCredentialService.DefaultUsername);
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
                //                await _dialogService.ShowMessageDialogAsync("Something happended...!", "User info not found");
                return result;
            }

            var user = userResponse.Result;

            var response = await _dataService
                .UserService
                .ExistsAsync(u => u.GoogleUserID == user.ID);

            var now = DateTimeOffset.UtcNow;
            ResponseDto<GoogleUser> userSaved;
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
                    //                    await _dialogService.ShowMessageDialogAsync("Error", userInDbResponse.Message);
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
                //                await _dialogService.ShowMessageDialogAsync(
                //                    "Error",
                //                    $"The user could not be saved / updated into the db. Error = {userSaved.Message}");
                return result;
            }

            _logger.Information($"{nameof(SignInAsync)}: Trying to get all the task lists that are remote...");
            //            var syncResult = await _syncService.SyncDownTaskListsAsync(false);
            //
            //            if (!syncResult.Succeed)
            //            {
            ////                await _dialogService
            ////                    .ShowMessageDialogAsync("Error", syncResult.Message);
            //                return result;
            //            }
            //
            //            _logger.Information($"{nameof(SignInAsync)}: Trying to get all the tasks that are remote...");
            //            syncResult = await _syncService.SyncDownTasksAsync(false);
            //
            //            if (!syncResult.Succeed)
            //            {
            ////                await _dialogService
            ////                    .ShowMessageDialogAsync("Error", syncResult.Message);
            //                return result;
            //            }

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

            //            await _googleUserService.DownloadProfileImage(user.ImageUrl, user.ID);

            //if you came this far, that means everything is ok!
            await _navigationService.Navigate<MainViewModel>();
            return !result;
        }

        public async Task InitViewAsync()
        {
            ShowLoading = true;
            var response = await _dataService
                .UserService
                .GetCurrentActiveUserAsync();

            var _dialogService = new DialogService();

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
                _dialogService.ShowInfoToast(
                    $"Coudln't retrieve the current logged user.{Environment.NewLine}Error = {errorMsg}");
                //                await _dialogService.(
                //                    "Error",
                //                    $"Coudln't retrieve the current logged user.{Environment.NewLine}Error = {errorMsg}");
                //                _userCredentialService.DeleteUserCredential(
                //                    PasswordVaultResourceType.ALL,
                //                    _userCredentialService.DefaultUsername);

                //                _logger.Warning($"{nameof(InitViewAsync)}: Couldnt get a user in the db = {response.Succeed} " +
                //                                $"or isUserLoggedIn and no user exists in db. {errorMsg}");
                return;
            }

            ShowLoading = false;

            if (isUserLoggedIn && _appSettings.AskForPasswordWhenAppStarts)
            {
                //                var result = await _dialogService.ShowCustomDialog(CustomDialogType.LOGIN_PASSWORD_DIALOG);
                //                if (result)
                //                    _navigationService.NavigateTo(ViewModelLocator.HOME_PAGE);
                //                else
                //                    ShowLoginButton = true;
            }
            else if (isUserLoggedIn)
            {
                _dialogService.ShowInfoToast("navegando...");
                await _navigationService.Navigate<MainViewModel>();
            }
            else
            {
                ShowLoginButton = true;
            }
        }
    }
}