using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Views;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Security.Authentication.Web;
using Windows.Security.Credentials;

namespace MiraiNotes.UWP.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        #region Members
        private bool _showLoading;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IGoogleUserService _googleUserService;
        #endregion

        #region Properties
        public bool ShowLoading
        {
            get { return _showLoading; }
            set { SetValue(ref _showLoading, value); }
        }
        #endregion

        #region Commands
        public ICommand LoginCommand
        {
            get
            {
                return new RelayCommand(SignInWithGoogleAsync);
            }
        }

        public ICommand LoadedCommand
        {
            get
            {
                return new RelayCommand(() => 
                {
                    bool isUserLoggedIn = _userCredentialService.IsUserLoggedIn();
                    if (isUserLoggedIn)
                    {
                        _navigationService.NavigateTo(ViewModelLocator.HOME_PAGE);
                    }
                });
            }
        }
        #endregion

        #region Constructors
        public LoginViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            IUserCredentialService userCredentialService, 
            IGoogleAuthService googleAuthService,
            IGoogleUserService googleUserService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            _userCredentialService = userCredentialService;
            _googleAuthService = googleAuthService;
            _googleUserService = googleUserService;
        }
        #endregion

        #region Methods    
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
                            await _dialogService.ShowMessage($"OAuth authorization error: {queryParams["error"]}", "Error");
                            return;
                        }

                        if (!queryParams.ContainsKey("approvalCode"))
                        {
                            await _dialogService.ShowMessage("Malformed authorization response.", "Error");
                            return;
                        }

                        // Gets the Authorization code
                        string approvalCode = queryParams["approvalCode"];
                        var tokenResponse = await _googleAuthService.GetTokenAsync(approvalCode);
                        if (tokenResponse == null)
                        {
                            await _dialogService.ShowMessage("Couldn't get a token", "Something happended...!");
                            return;
                        }

                        _userCredentialService.SaveUserCredentials(null, tokenResponse);
                        var user = await _googleUserService.GetUserInfoAsync(tokenResponse.AccessToken);
                        if (user == null)
                        {
                            await _dialogService.ShowMessage("User info not found", "Something happended...!");
                            return;
                        }
                        
                        //TODO: SAVE THE USER TO THE DB
                        _navigationService.NavigateTo(ViewModelLocator.HOME_PAGE);
                        break;
                    case WebAuthenticationStatus.UserCancel:
                        break;
                    case WebAuthenticationStatus.ErrorHttp:
                        await _dialogService.ShowMessage(result.ResponseErrorDetail.ToString(), "Error");
                        break;
                    default:
                        await _dialogService.ShowMessage(result.ResponseData.ToString(), "Error");
                        break;
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowError(ex, "An unknown error occurred", null, () => { });
            }
            finally
            {
                ShowLoading = false;
            }
        }
        #endregion
    }
}
