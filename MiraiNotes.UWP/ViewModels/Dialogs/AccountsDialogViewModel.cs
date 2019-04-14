using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MiraiNotes.Data.Models;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.Shared.Models;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Security.Authentication.Web;

namespace MiraiNotes.UWP.ViewModels.Dialogs
{
    public class AccountsDialogViewModel : ViewModelBase
    {
        private readonly IMessenger _messenger;
        private readonly IMapper _mapper;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IGoogleUserService _googleUserService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly ICustomDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly INetworkService _networkService;
        private readonly ISyncService _syncService;

        private ObservableCollection<GoogleUserViewModel> _accounts =
            new ObservableCollection<GoogleUserViewModel>();

        public ICommand LoadedCommand { get; private set; }

        public ICommand AddAccountCommand { get; private set; }

        public ICommand ChangeCurrentAccountCommand { get; private set; }

        public ICommand DeleteAccountCommand { get; private set; }

        public ObservableCollection<GoogleUserViewModel> Accounts
        {
            get => _accounts;
            set => Set(ref _accounts, value);
        }

        public AccountsDialogViewModel(
            IMessenger messenger,
            IMapper mapper,
            IGoogleAuthService googleAuthService,
            IGoogleUserService googleUserService,
            IUserCredentialService userCredentialService,
            ICustomDialogService dialogService,
            IMiraiNotesDataService dataService,
            INetworkService networkService,
            ISyncService syncService)
        {
            _messenger = messenger;
            _mapper = mapper;
            _googleAuthService = googleAuthService;
            _googleUserService = googleUserService;
            _userCredentialService = userCredentialService;
            _dialogService = dialogService;
            _dataService = dataService;
            _networkService = networkService;
            _syncService = syncService;
            RegisterCommands();
        }

        //TODO: IF WE CHANGE ACCOUNTS, THE TASK PAGE IS NOT GETTING CLEANED, ONLY THE TASKLIST
        //TODO: GOOGLEUSERVIEWMODEL PROPERTIES MUST RAISE THE PROPERTY CHANGED
        //TODO: IMG SHOULD BE CACHED, INSTEAD OF BEING DOWNLOADED BY THE URL
        //TODO: IF THE SAME EMAIL IS ALREADY IN DB WE NEED TO VALIDATE IT
        //TODO: THIS SHIT CRASH IF WE SHOW ANOTHER DIALOG
        //TODO: SHOULD I STOP RUNNING A BG TASK? BTW THE TASK THAT WILL ONLY GET SYNCED ARE FOR THE CURRENT ACCOUNT
        //TODO: IF WE SWITCH ACCOUNTS, SHOULD I CALL THE SYNC SERVICE?

        private void RegisterCommands()
        {
            LoadedCommand = new RelayCommand
                (async () => await InitAsync());

            AddAccountCommand = new RelayCommand
                (async () => await AddAccountAsync());

            ChangeCurrentAccountCommand = new RelayCommand<int>
                (async id => await ChangeCurrentAccountAsync(id));

            DeleteAccountCommand = new RelayCommand<GoogleUserViewModel>
                (async account => await DeleteAccountAsync(account));
        }

        public async Task InitAsync()
        {
            var response = await _dataService.UserService.GetAllAsNoTrackingAsync();
            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    "An error occurred while trying to load the user accounts");
                return;
            }

            var accounts = _mapper.Map<IEnumerable<GoogleUserViewModel>>(response.Result);
            Accounts = new ObservableCollection<GoogleUserViewModel>(accounts);
        }

        public async Task AddAccountAsync()
        {
            if (!_networkService.IsInternetAvailable())
            {
                await _dialogService.ShowMessageDialogAsync("Error", "Network is not available");
                return;
            }
            _messenger.Send(new Tuple<bool, string>(true, null), $"{MessageType.SHOW_MAIN_PROGRESS_BAR}");
            var requestUri = new Uri(_googleAuthService.GetAuthorizationUrl());
            var callbackUri = new Uri(_googleAuthService.ApprovalUrl);
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

                        //We temporaly save and asociate the token to a default user 
                        //before doing any other network requst..
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

                        //TODO: I NEED TO DO SOMETHING WHEN THIS SHIT FAILS
                        var user = await SignInAsync();
                        if (user is null)
                        {
                            await _dataService
                                .UserService
                                .ChangeCurrentUserStatus(false);

                            _userCredentialService.DeleteUserCredential(
                                PasswordVaultResourceType.ALL,
                                _userCredentialService.DefaultUsername);
                            return;
                        }
                        ChangeIsActiveStatus(false);
                        Accounts.Add(user);
                        _messenger.Send(user.Fullname, $"{MessageType.CURRENT_USER_CHANGED}");
                        break;
                    case WebAuthenticationStatus.UserCancel:
                        break;
                    case WebAuthenticationStatus.ErrorHttp:
                        await _dialogService.ShowMessageDialogAsync("Error", result.ResponseErrorDetail.ToString());
                        break;
                    default:
                        await _dialogService.ShowMessageDialogAsync("Error", result.ResponseData);
                        break;
                }
            }
            catch (Exception ex)
            {
                //_userCredentialService.DeleteUserCredential(
                //    PasswordVaultResourceType.ALL,
                //    _userCredentialService.DefaultUsername);
                await _dialogService.ShowErrorMessageDialogAsync(ex, "An unknown error occurred");
            }
            finally
            {
                _messenger.Send(new Tuple<bool, string>(false, null), $"{MessageType.SHOW_MAIN_PROGRESS_BAR}");
            }
        }

        private async Task<GoogleUserViewModel> SignInAsync()
        {
            var user = await _googleUserService.GetUserInfoAsync();
            if (user == null)
            {
                await _dialogService.ShowMessageDialogAsync("Something happended...!", "User info not found");
                return null;
            }

            await _dataService
                .UserService
                .ChangeCurrentUserStatus(false);

            var response = await _dataService
                .UserService
                .ExistsAsync(u => u.GoogleUserID == user.ID);
            Response<GoogleUser> userSaveResponse;
            if (!response.Result)
            {
                userSaveResponse = await _dataService.UserService.AddAsync(new GoogleUser
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
                    return null;
                }

                userInDbResponse.Result.Fullname = user.FullName;
                userInDbResponse.Result.Email = user.Email;
                userInDbResponse.Result.IsActive = true;
                userInDbResponse.Result.PictureUrl = user.ImageUrl;

                userSaveResponse = await _dataService
                    .UserService
                    .UpdateAsync(userInDbResponse.Result);
            }

            if (!userSaveResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"The user could not be saved / updated into the db. Error = {userSaveResponse.Message}");
                return null;
            }

            var syncResult = await _syncService.SyncDownTaskListsAsync(false);

            if (!syncResult.Succeed)
            {
                await _dialogService
                    .ShowMessageDialogAsync("Error", syncResult.Message);
                return null;
            }

            syncResult = await _syncService.SyncDownTasksAsync(false);

            if (!syncResult.Succeed)
            {
                await _dialogService
                    .ShowMessageDialogAsync("Error", syncResult.Message);
                return null;
            }

            _userCredentialService.UpdateUserCredential(
                PasswordVaultResourceType.LOGGED_USER_RESOURCE,
                _userCredentialService.DefaultUsername,
                false,
                userSaveResponse.Result.Email);
            _userCredentialService.UpdateUserCredential(
                PasswordVaultResourceType.REFRESH_TOKEN_RESOURCE,
                _userCredentialService.DefaultUsername,
                true,
                userSaveResponse.Result.Email);
            _userCredentialService.UpdateUserCredential(
                PasswordVaultResourceType.TOKEN_RESOURCE,
                _userCredentialService.DefaultUsername,
                true,
                userSaveResponse.Result.Email);

            await _googleUserService.RemoveProfileImage();

            await _googleUserService.DownloadProfileImage(user.ImageUrl);

            return _mapper.Map<GoogleUserViewModel>(userSaveResponse.Result);
        }

        private async Task ChangeCurrentAccountAsync(int id)
        {
            await _dataService
                .UserService
                .ChangeCurrentUserStatus(false);

            var userInDbResponse = await _dataService
                .UserService
                .FirstOrDefaultAsNoTrackingAsync(u => u.ID == id);

            if (!userInDbResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync("Error", userInDbResponse.Message);
                return;
            }

            userInDbResponse.Result.IsActive = true;

            var userSaveResponse = await _dataService
                .UserService
                .UpdateAsync(userInDbResponse.Result);

            if (!userSaveResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    $"The user could not be saved / updated into the db. Error = {userSaveResponse.Message}");
                return;
            }
            _userCredentialService.UpdateUserCredential(
                PasswordVaultResourceType.LOGGED_USER_RESOURCE,
                _userCredentialService.DefaultUsername,
                false,
                userInDbResponse.Result.Email);

            ChangeIsActiveStatus(false, id);
            _messenger.Send(userSaveResponse.Result.Fullname, $"{MessageType.CURRENT_USER_CHANGED}");
        }

        private async Task DeleteAccountAsync(GoogleUserViewModel account)
        {
            var userInDbResponse = await _dataService
                .UserService
                .FirstOrDefaultAsNoTrackingAsync(u => u.ID == account.ID);

            if (!userInDbResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync("Error", userInDbResponse.Message);
                return;
            }

            //bool confirmed = await _dialogService.ShowConfirmationDialogAsync(
            //    "Confirmation",
            //    $"Are you sure you wanna delete {userInDbResponse.Result.Email} from your accounts?",
            //    "Yes",
            //    "No");

            //if (!confirmed)
            //    return;

            if (userInDbResponse.Result.IsActive && Accounts.Count == 1)
            {
                await _dialogService.ShowMessageDialogAsync("Error", "You can not delete the default account");
                return;
            }
            else if (userInDbResponse.Result.IsActive)
            {
                //esto creo q revienta...
                int nextIndex = 0;
                int currentIndex = Accounts.IndexOf(account);
                var next = Accounts.ElementAtOrDefault(currentIndex + 1);
                var previous = Accounts.ElementAtOrDefault(currentIndex - 1);

                if (previous != null)
                    nextIndex = currentIndex - 1;
                else if (next != null)
                    nextIndex = currentIndex + 1;


                var defaultAccount = Accounts[nextIndex];

                var userInDbResponse2 = await _dataService
                    .UserService
                    .FirstOrDefaultAsNoTrackingAsync(u => u.ID == defaultAccount.ID);

                if (!userInDbResponse2.Succeed)
                {
                    await _dialogService.ShowMessageDialogAsync("Error", userInDbResponse2.Message);
                    return;
                }

                userInDbResponse2.Result.IsActive = true;
                var updatedResponse = await _dataService.UserService
                    .UpdateAsync(userInDbResponse2.Result);

                if (!updatedResponse.Succeed)
                {
                    await _dialogService.ShowMessageDialogAsync("Error", updatedResponse.Message);
                    return;
                }

                defaultAccount.IsActive = true;

                _userCredentialService.UpdateUserCredential(
                    PasswordVaultResourceType.LOGGED_USER_RESOURCE,
                    _userCredentialService.DefaultUsername,
                    false,
                    defaultAccount.Email);
                _messenger.Send(defaultAccount.Fullname, $"{MessageType.CURRENT_USER_CHANGED}");
            }

            _userCredentialService.DeleteUserCredential(
                PasswordVaultResourceType.REFRESH_TOKEN_RESOURCE, 
                userInDbResponse.Result.Email);

            _userCredentialService.DeleteUserCredential(
                PasswordVaultResourceType.TOKEN_RESOURCE,
                userInDbResponse.Result.Email);

            var deleteResponse = await _dataService.UserService.RemoveAsync(account.ID);
            if (deleteResponse.Succeed)
            {
                Accounts.Remove(account);
            }
            else
            {
                await _dialogService.ShowMessageDialogAsync("Error", deleteResponse.Message);
            }
        }

        private void ChangeIsActiveStatus(bool isActive, int? exceptId = null)
        {
            foreach (var account in Accounts)
            {
                if (exceptId.HasValue && account.ID == exceptId.Value)
                    account.IsActive = !isActive;
                else
                    account.IsActive = isActive;
            }
        }
    }
}
