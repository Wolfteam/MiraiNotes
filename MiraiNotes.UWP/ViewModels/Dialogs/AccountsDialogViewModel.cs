using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MiraiNotes.Data.Models;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.Shared.Models;
using MiraiNotes.UWP.Delegates;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiraiNotes.UWP.ViewModels.Dialogs
{
    public class AccountsDialogViewModel : ViewModelBase
    {
        #region Members
        private readonly ILogger _logger;
        private readonly IMessenger _messenger;
        private readonly IMapper _mapper;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IGoogleUserService _googleUserService;
        private readonly IUserCredentialService _userCredentialService;
        private readonly ICustomDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly INetworkService _networkService;
        private readonly ISyncService _syncService;
        private readonly IApplicationSettingsService _appSettings;
        private ObservableCollection<GoogleUserViewModel> _accounts =
            new ObservableCollection<GoogleUserViewModel>();

        #endregion

        #region Properties
        public ObservableCollection<GoogleUserViewModel> Accounts
        {
            get => _accounts;
            private set => Set(ref _accounts, value);
        }

        #endregion

        #region Commands
        public ICommand LoadedCommand { get; private set; }

        public ICommand AddAccountCommand { get; private set; }

        public ICommand ChangeCurrentAccountCommand { get; private set; }

        public ICommand DeleteAccountCommand { get; private set; }

        #endregion

        #region Events
        public HideDialog HideDialogRequest;
        #endregion

        public AccountsDialogViewModel(
            ILogger logger,
            IMessenger messenger,
            IMapper mapper,
            IGoogleAuthService googleAuthService,
            IGoogleUserService googleUserService,
            IUserCredentialService userCredentialService,
            ICustomDialogService dialogService,
            IMiraiNotesDataService dataService,
            INetworkService networkService,
            ISyncService syncService,
            IApplicationSettingsService appSettingsService)
        {
            _logger = logger;
            _messenger = messenger;
            _mapper = mapper;
            _googleAuthService = googleAuthService;
            _googleUserService = googleUserService;
            _userCredentialService = userCredentialService;
            _dialogService = dialogService;
            _dataService = dataService;
            _networkService = networkService;
            _syncService = syncService;
            _appSettings = appSettingsService;
            RegisterCommands();
        }

        //TODO: MOVE THE SIGNIN TO A COMMON METHOD

        #region Methods
        private void RegisterCommands()
        {
            LoadedCommand = new RelayCommand
                (async () => await InitAsync());

            AddAccountCommand = new RelayCommand
                (async () => await AddAccountAsync());

            ChangeCurrentAccountCommand = new RelayCommand<GoogleUserViewModel>(async account =>
            {
                HideDialogRequest.Invoke();
                _messenger.Send(new Tuple<bool, string>(true, $"Switching to {account.Email}..."), $"{MessageType.SHOW_MAIN_PROGRESS_BAR}");
                await ChangeCurrentAccountAsync(account.ID);
                _messenger.Send(new Tuple<bool, string>(false, null), $"{MessageType.SHOW_MAIN_PROGRESS_BAR}");
            });

            DeleteAccountCommand = new RelayCommand<GoogleUserViewModel>(async account =>
            {
                HideDialogRequest.Invoke();

                bool confirmed = await _dialogService.ShowConfirmationDialogAsync(
                    "Confirmation",
                    $"Are you sure you wanna delete {account.Email} from your accounts?",
                    "Yes",
                    "No");

                if (!confirmed)
                    return;
                _messenger.Send(new Tuple<bool, string>(true, $"Deleting {account.Email}..."), $"{MessageType.SHOW_MAIN_PROGRESS_BAR}");
                await DeleteAccountAsync(account);
                _messenger.Send(new Tuple<bool, string>(false, null), $"{MessageType.SHOW_MAIN_PROGRESS_BAR}");
            });
        }

        public async Task InitAsync()
        {
            _logger.Information($"{nameof(InitAsync)}: Loading all the saved users in db");
            var response = await _dataService.UserService.GetAllAsNoTrackingAsync();
            if (!response.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync(
                    "Error",
                    "An error occurred while trying to load the user accounts");
                return;
            }
            var accounts = new List<GoogleUserViewModel>();
            foreach (var user in response.Result)
            {
                var vm = new GoogleUserViewModel(_googleUserService);
                accounts.Add(_mapper.Map(user, vm));
            }
            Accounts = new ObservableCollection<GoogleUserViewModel>(accounts);
        }

        public async Task AddAccountAsync()
        {
            if (!_networkService.IsInternetAvailable())
            {
                await _dialogService.ShowMessageDialogAsync("Error", "Network is not available");
                return;
            }

            _messenger.Send(new Tuple<bool, string>(true, "Adding a new account..."), $"{MessageType.SHOW_MAIN_PROGRESS_BAR}");
            _logger.Information($"{nameof(AddAccountAsync)}: Trying to sign in with google...");
            var response = await _googleAuthService.SignInWithGoogle();
            await OnGoogleSignInResponse(response);

            _messenger.Send(new Tuple<bool, string>(false, null), $"{MessageType.SHOW_MAIN_PROGRESS_BAR}");
        }

        private async Task OnGoogleSignInResponse(Response<TokenResponse> response)
        {
            string currentUser = _userCredentialService.GetCurrentLoggedUsername();
            try
            {
                if (!response.Succeed)
                {
                    //user canceled auth..
                    if (string.IsNullOrEmpty(response.Message))
                        return;
                    _logger.Error($"{nameof(OnGoogleSignInResponse)}: The token response failed. {response.Message}");
                    await _dialogService.ShowMessageDialogAsync("Error", response.Message);
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
                    response.Result.RefreshToken);

                _userCredentialService.SaveUserCredential(
                    PasswordVaultResourceType.TOKEN_RESOURCE,
                    _userCredentialService.DefaultUsername,
                    response.Result.AccessToken);

                var user = await SignInAsync();
                if (user is null)
                {
                    throw new Exception("An error occurred while trying to sign in into the app");
                }

                ChangeIsActiveStatus(false);
                Accounts.Add(user);
                _messenger.Send(user.Fullname, $"{MessageType.CURRENT_ACTIVE_USER_CHANGED}");
            }
            catch (Exception ex)
            {
                await _dataService
                    .UserService
                    .SetAsCurrentUser(currentUser);

                _userCredentialService.DeleteUserCredential(
                    PasswordVaultResourceType.ALL,
                    _userCredentialService.DefaultUsername);

                _userCredentialService.SaveUserCredential(
                    PasswordVaultResourceType.LOGGED_USER_RESOURCE,
                    _userCredentialService.DefaultUsername,
                    currentUser);
                _logger.Error(ex, $"{nameof(OnGoogleSignInResponse)}: An unknown error occurred while signing in the app");
                await _dialogService.ShowErrorMessageDialogAsync(ex, "An unknown error occurred");
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

            var userExistsResponse = await _dataService.UserService
                .ExistsAsync(u => u.GoogleUserID == user.ID);

            if (!userExistsResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync("Error", "Couldnt check if user already exists in the db");
                return null;
            }

            //if the user is trying to add an existing account
            if (userExistsResponse.Result)
            {
                await _dialogService.ShowMessageDialogAsync("Error", $"{user.Email} already exsits in the db");
                return null;
            }

            await _dataService
                .UserService
                .ChangeCurrentUserStatus(false);

            var response = await _dataService
                .UserService
                .ExistsAsync(u => u.GoogleUserID == user.ID);

            var now = DateTimeOffset.UtcNow;
            Response<GoogleUser> userSaveResponse;
            if (!response.Result)
            {
                userSaveResponse = await _dataService.UserService.AddAsync(new GoogleUser
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
                userInDbResponse.Result.UpdatedAt = now;

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

            _logger.Information($"{nameof(SignInAsync)}: Trying to get all the task lists that are remote...");
            var syncResult = await _syncService.SyncDownTaskListsAsync(false);

            if (!syncResult.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync("Error", syncResult.Message);
                return null;
            }

            _logger.Information($"{nameof(SignInAsync)}: Trying to get all the tasks that are remote...");
            syncResult = await _syncService.SyncDownTasksAsync(false);

            if (!syncResult.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync("Error", syncResult.Message);
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

            await _googleUserService.DownloadProfileImage(user.ImageUrl, user.ID);

            return _mapper.Map<GoogleUserViewModel>(userSaveResponse.Result);
        }

        private async Task ChangeCurrentAccountAsync(int id)
        {
            _logger.Information($"{nameof(ChangeCurrentAccountAsync)}: Changing current active user to userId = {id}");
            var userInDbResponse = await _dataService
                .UserService
                .FirstOrDefaultAsNoTrackingAsync(u => u.ID == id);

            if (!userInDbResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync("Error", userInDbResponse.Message);
                return;
            }

            await _dataService
                .UserService
                .ChangeCurrentUserStatus(false);

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

            bool isNetworkAvailable = _networkService.IsInternetAvailable();
            if (isNetworkAvailable && _appSettings.RunFullSyncAfterSwitchingAccounts)
            {
                var syncResults = new List<EmptyResponse>
                {
                    await _syncService.SyncDownTaskListsAsync(false),
                    await _syncService.SyncDownTasksAsync(false),
                    await _syncService.SyncUpTaskListsAsync(false),
                    await _syncService.SyncUpTasksAsync(false)
                };

                string message = syncResults.Any(r => !r.Succeed)
                    ? string.Join($".{Environment.NewLine}", syncResults.Where(r => !r.Succeed).Select(r => r.Message).Distinct())
                    : "A automatic full sync was successfully performed.";

                if (string.IsNullOrEmpty(message))
                    message = "An unknown error occurred while trying to perform the sync operation.";

                _messenger.Send(message, $"{MessageType.SHOW_IN_APP_NOTIFICATION}");
            }
            else if (!isNetworkAvailable)
            {
                _messenger.Send(
                    "Internet is not available, a full sync will not be performed", 
                    $"{MessageType.SHOW_IN_APP_NOTIFICATION}");
            }
            ChangeIsActiveStatus(false, id);
            _messenger.Send(userSaveResponse.Result.Fullname, $"{MessageType.CURRENT_ACTIVE_USER_CHANGED}");
        }

        private async Task DeleteAccountAsync(GoogleUserViewModel account)
        {
            _logger.Information($"{nameof(DeleteAccountAsync)}: Deleting user = {account.Email}...");

            var userInDbResponse = await _dataService
                .UserService
                .FirstOrDefaultAsNoTrackingAsync(u => u.ID == account.ID);

            if (!userInDbResponse.Succeed)
            {
                await _dialogService.ShowMessageDialogAsync("Error", userInDbResponse.Message);
                return;
            }

            if (userInDbResponse.Result.IsActive && Accounts.Count == 1)
            {
                await _dialogService.ShowMessageDialogAsync("Error", "You can not delete the default account");
                return;
            }

            if (userInDbResponse.Result.IsActive)
            {
                _logger.Information($"{nameof(DeleteAccountAsync)}: User = {account.Email} " +
                    $"is active, setting the active flag to another user");
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

                _logger.Information($"{nameof(DeleteAccountAsync)}: User = {defaultAccount.Email} will be marked as active");

                var response = await _dataService
                    .UserService
                    .SetAsCurrentUser(defaultAccount.Email);

                if (!response.Succeed)
                {
                    await _dialogService.ShowMessageDialogAsync(
                        "Error",
                        $"Couldnt set {defaultAccount.Email} as default. Error = {response.Message}");
                    return;
                }

                defaultAccount.IsActive = true;

                _userCredentialService.UpdateUserCredential(
                    PasswordVaultResourceType.LOGGED_USER_RESOURCE,
                    _userCredentialService.DefaultUsername,
                    false,
                    defaultAccount.Email);
                _messenger.Send(defaultAccount.Fullname, $"{MessageType.CURRENT_ACTIVE_USER_CHANGED}");
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

        #endregion
    }
}