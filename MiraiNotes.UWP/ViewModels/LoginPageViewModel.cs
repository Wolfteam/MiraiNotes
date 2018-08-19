﻿using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.UWP.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Security.Authentication.Web;

namespace MiraiNotes.UWP.ViewModels
{
    public class LoginPageViewModel : BaseViewModel
    {
        #region Members
        private bool _showLoading;
        private readonly ICustomDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IGoogleUserService _googleUserService;

        private readonly IMiraiNotesDataService _dataService;
        private readonly INetworkService _networkService;
        private readonly ISyncService _syncService;
        #endregion

        #region Properties
        public bool ShowLoading
        {
            get { return _showLoading; }
            set { SetValue(ref _showLoading, value); }
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
            ISyncService syncService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userCredentialService = userCredentialService;
            _googleAuthService = googleAuthService;
            _googleUserService = googleUserService;
            _dataService = dataService;
            _networkService = networkService;
            _syncService = syncService;

            SetCommands();
        }
        #endregion

        #region Methods    

        private void SetCommands()
        {
            LoadedCommand = new RelayCommand(async() =>
            {
                bool isUserLoggedIn = _userCredentialService.IsUserLoggedIn();
                var currentUser = await _dataService
                    .UserService
                    .GetCurrentActiveUserAsync();
                if (isUserLoggedIn && currentUser != null)
                {
                    _navigationService.NavigateTo(ViewModelLocator.HOME_PAGE);
                }
            });
            LoginCommand = new RelayCommand(SignInWithGoogleAsync);

        }

        public async void SignInWithGoogleAsync()
        {
            ShowLoading = true;
            string googleUrl = _googleAuthService.GetAuthorizationUrl();
            Uri requestUri = new Uri(googleUrl);
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
                        _userCredentialService.SaveUserCredentials(null, tokenResponse);

                        await SignInAsync();
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
                _userCredentialService.DeleteUserCredentials();
                await _dialogService.ShowErrorMessageDialogAsync(ex, "An unknown error occurred");
            }
            finally
            {
                ShowLoading = false;
            }
        }

        private async Task SignInAsync()
        {
            var user = await _googleUserService.GetUserInfoAsync();
            if (user == null)
            {
                _userCredentialService.DeleteUserCredentials();
                await _dialogService.ShowMessageDialogAsync("Something happended...!", "User info not found");
                return;
            }

            bool userIsAlreadySaved = await _dataService
                .UserService
                .ExistsAsync(u => u.GoogleUserID == user.ID);

            if (!userIsAlreadySaved)
            {
                await _dataService.UserService.AddAsync(new Data.Models.GoogleUser
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
                var userInDb = (await _dataService
                    .UserService
                    .GetAsync(u => u.GoogleUserID == user.ID, null, string.Empty))
                    .FirstOrDefault();

                userInDb.Fullname = user.FullName;
                userInDb.Email = user.Email;
                userInDb.IsActive = true;
                userInDb.PictureUrl = user.ImageUrl;

                _dataService.UserService.Update(userInDb);
            }

            var userSaved = await _dataService.SaveChangesAsync();
            if (!userSaved.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error", 
                    $"The user could not be saved / updated into the db. Error = {userSaved.Message}");
                return;
            }
            var syncResult = await _syncService.SyncDownTaskListsAsync(false);

            if (!syncResult.Succeed)
            {
                await _dialogService
                    .ShowMessageDialogAsync("Error", syncResult.Message);
                return;
            }

            syncResult = await _syncService.SyncDownTasksAsync(false);

            if (!syncResult.Succeed)
            {
                await _dialogService
                    .ShowMessageDialogAsync("Error", syncResult.Message);
                return;
            }

            _navigationService.NavigateTo(ViewModelLocator.HOME_PAGE);
        }
        #endregion
    }
}
