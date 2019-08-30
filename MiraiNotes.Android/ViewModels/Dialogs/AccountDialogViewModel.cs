using AutoMapper;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Entities;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class AccountDialogViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly INetworkService _networkService;
        private readonly IAppSettingsService _appSettings;
        private readonly ISyncService _syncService;
        private readonly IGoogleApiService _googleApiService;
        private readonly IUserCredentialService _userCredentialService;

        private MvxObservableCollection<GoogleUserViewModel> _accounts = new MvxObservableCollection<GoogleUserViewModel>();
        private bool _isDialogVisible = true;
        private readonly MvxInteraction<string> _onAddAccountRequest = new MvxInteraction<string>();

        public IMvxInteraction<string> OnAddAccountRequest
            => _onAddAccountRequest;

        public MvxObservableCollection<GoogleUserViewModel> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        public bool IsDialogVisible
        {
            get => _isDialogVisible;
            set => SetProperty(ref _isDialogVisible, value);
        }

        public IMvxCommand NewAccountCommand { get; private set; }
        public IMvxAsyncCommand CloseCommand { get; private set; }
        public IMvxAsyncCommand<string> OnAuthCodeGrantedCommand { get; private set; }

        public AccountDialogViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            IMapper mapper,
            ILogger logger,
            IDialogService dialogService,
            IMiraiNotesDataService dataService,
            INetworkService networkService,
            IAppSettingsService appSettings,
            ISyncService syncService,
            IGoogleApiService googleApiService,
            IUserCredentialService userCredentialService)
            : base(textProvider, messenger)
        {
            _navigationService = navigationService;
            _mapper = mapper;
            _logger = logger.ForContext<AccountDialogViewModel>();
            _dialogService = dialogService;
            _dataService = dataService;
            _networkService = networkService;
            _appSettings = appSettings;
            _syncService = syncService;
            _googleApiService = googleApiService;
            _userCredentialService = userCredentialService;

            SetCommands();

            RegisterMessages();
        }


        public override async Task Initialize()
        {
            await base.Initialize();
            await InitView();
        }

        private void SetCommands()
        {
            NewAccountCommand = new MvxCommand(AddAccountRequest);
            CloseCommand = new MvxAsyncCommand(async () => await _navigationService.Close(this));
            OnAuthCodeGrantedCommand = new MvxAsyncCommand<string>(OnCodeGranted);
        }

        private void RegisterMessages()
        {
            var tokens = new[]
            {
                Messenger.Subscribe<AccountChangeRequestMsg>(async (msg) => 
                {
                    if (msg.DeleteAccount)
                        await DeleteAccount(msg.Account);
                    else
                        await ChangeActiveAccount(msg.Account.Id);
                })
            };

            SubscriptionTokens.AddRange(tokens);
        }

        private async Task InitView()
        {
            var dbResponse = await _dataService.UserService
                .GetAsNoTrackingAsync(orderBy: user => user.OrderBy(u => u.Fullname));

            if (!dbResponse.Succeed)
            {
                _dialogService.ShowErrorToast(
                    $"An unknown error occurred while trying to retrieve all the task lists from the db. Error = {dbResponse.Message}");
                return;
            }

            Accounts = _mapper.Map<MvxObservableCollection<GoogleUserViewModel>>(dbResponse.Result);

            SetCanBeDeleted();
        }

        public void AddAccountRequest()
        {
            if (!_networkService.IsInternetAvailable())
            {
                _dialogService.ShowSnackBar("Network is not available", string.Empty);
                return;
            }
            var url = _googleApiService.GetAuthorizationUrl();
            _onAddAccountRequest.Raise(url);
            //await _navigationService.Close(this);
        }

        private async Task OnCodeGranted(string code)
        {
            string currentUser = _userCredentialService.GetCurrentLoggedUsername();
            try
            {
                IsDialogVisible = false;
                Messenger.Publish(new ShowProgressOverlayMsg(this));
                var response = await _googleApiService.GetAccessTokenAsync(code);

                if (!response.Succeed)
                {
                    //user canceled auth..
                    if (string.IsNullOrEmpty(response.Message))
                        return;
                    _logger.Error(
                        $"{nameof(OnCodeGranted)}: The token response failed. {response.Message}");
                    _dialogService.ShowSnackBar(response.Message);
                    return;
                }

                //We temporaly save and asociate the token to a default user 
                //before doing any other network requst..
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

                var user = await SignIn();
                if (user is null)
                {
                    throw new Exception("An error occurred while trying to sign in into the app");
                }

                ChangeIsActiveStatus(false);
                Messenger.Publish(new ActiveAccountChangedMsg(this));
                Accounts.Add(user);
            }
            catch (Exception ex)
            {
                await _dataService
                    .UserService
                    .SetAsCurrentUser(currentUser);

                _userCredentialService.DeleteUserCredential(
                    ResourceType.ALL,
                    _userCredentialService.DefaultUsername);

                _userCredentialService.SaveUserCredential(
                    ResourceType.LOGGED_USER_RESOURCE,
                    _userCredentialService.DefaultUsername,
                    currentUser);
                _logger.Error(ex, 
                    $"{nameof(OnCodeGranted)}: An unknown error occurred while signing in the app");
                _dialogService.ShowSnackBar($"An unknown error occurred. Ex = {ex.Message}");
            }
            finally
            {
                SetCanBeDeleted();
                Messenger.Publish(new ShowProgressOverlayMsg(this, false));
                IsDialogVisible = true;
            }
        }

        private async Task<GoogleUserViewModel> SignIn()
        {
            _logger.Information(
                $"{nameof(SignIn)}: Sign in the app started. Trying to get the user info from google");

            var userResponse = await _googleApiService.GetUser();
            if (!userResponse.Succeed)
            {
                _dialogService.ShowSnackBar("User info not found");
                return null;
            }

            var user = userResponse.Result;

            var userExistsResponse = await _dataService
                .UserService
                .ExistsAsync(u => u.GoogleUserID == user.ID);

            if (!userExistsResponse.Succeed)
            {
                _dialogService.ShowSnackBar("Couldnt check if user already exists in the db");
                return null;
            }

            await _dataService
                .UserService
                .ChangeCurrentUserStatus(false);

            var response = await _dataService
                .UserService
                .ExistsAsync(u => u.GoogleUserID == user.ID);

            var now = DateTimeOffset.UtcNow;
            ResponseDto<GoogleUser> userSaveResponse;
            if (!response.Result)
            {
                _logger.Information($"{nameof(SignIn)}: User doesnt exist in db. Creating a new one...");
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
                _logger.Information($"{nameof(SignIn)}: User exist in db. Updating it...");
                var userInDbResponse = await _dataService
                    .UserService
                    .FirstOrDefaultAsNoTrackingAsync(u => u.GoogleUserID == user.ID);

                if (!userInDbResponse.Succeed)
                {
                    _dialogService.ShowSnackBar(userInDbResponse.Message);
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
                _dialogService.ShowSnackBar(
                    $"The user could not be saved / updated into the db. Error = {userSaveResponse.Message}");
                return null;
            }

            _logger.Information(
                $"{nameof(SignIn)}: trying to get all the task lists that are remote...");
            var syncResult = await _syncService.SyncDownTaskListsAsync(false);

            if (!syncResult.Succeed)
            {
                _dialogService.ShowSnackBar(syncResult.Message);
                return null;
            }

            _logger.Information(
                $"{nameof(SignIn)}: Trying to get all the tasks that are remote...");
            syncResult = await _syncService.SyncDownTasksAsync(false);

            if (!syncResult.Succeed)
            {
                _dialogService.ShowSnackBar(syncResult.Message);
                return null;
            }

            _userCredentialService.UpdateUserCredential(
                ResourceType.LOGGED_USER_RESOURCE,
                _userCredentialService.DefaultUsername,
                false,
                userSaveResponse.Result.Email);
            _userCredentialService.UpdateUserCredential(
                ResourceType.REFRESH_TOKEN_RESOURCE,
                _userCredentialService.DefaultUsername,
                true,
                userSaveResponse.Result.Email);
            _userCredentialService.UpdateUserCredential(
                ResourceType.TOKEN_RESOURCE,
                _userCredentialService.DefaultUsername,
                true,
                userSaveResponse.Result.Email);

            await MiscellaneousUtils.DownloadProfileImage(user.ImageUrl, user.ID);

            var vm = _mapper.Map<GoogleUserViewModel>(userSaveResponse.Result);
            vm.IsActive = true;
            return vm;
        }

        private async Task DeleteAccount(GoogleUserViewModel account)
        {
            _logger.Information(
                $"{nameof(DeleteAccount)}: Deleting user = {account.Email}...");

            var userInDbResponse = await _dataService
                .UserService
                .FirstOrDefaultAsNoTrackingAsync(u => u.ID == account.Id);

            if (!userInDbResponse.Succeed)
            {
                _dialogService.ShowSnackBar(userInDbResponse.Message);
                return;
            }

            var user = userInDbResponse.Result;

            if (user.IsActive && Accounts.Count == 1)
            {
                _dialogService.ShowSnackBar("You can not delete the default account");
                return;
            }

            if (user.IsActive)
            {
                _logger.Information(
                    $"{nameof(DeleteAccount)}: User = {account.Email} " +
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

                _logger.Information(
                    $"{nameof(DeleteAccount)}: User = {defaultAccount.Email} will be marked as active");

                var response = await _dataService
                    .UserService
                    .SetAsCurrentUser(defaultAccount.Email);

                if (!response.Succeed)
                {
                    _dialogService.ShowSnackBar(
                        $"Couldnt set {defaultAccount.Email} as default. Error = {response.Message}");
                    return;
                }

                defaultAccount.IsActive = true;

                _userCredentialService.UpdateUserCredential(
                    ResourceType.LOGGED_USER_RESOURCE,
                    _userCredentialService.DefaultUsername,
                    false,
                    defaultAccount.Email);
                Messenger.Publish(new ActiveAccountChangedMsg(this));
            }

            _userCredentialService.DeleteUserCredential(
                ResourceType.REFRESH_TOKEN_RESOURCE,
                user.Email);

            _userCredentialService.DeleteUserCredential(
                ResourceType.TOKEN_RESOURCE,
                user.Email);

            var deleteResponse = await _dataService.UserService.RemoveAsync(account.Id);
            if (deleteResponse.Succeed)
            {
                Accounts.Remove(account);
            }
            else
            {
                _dialogService.ShowSnackBar(deleteResponse.Message);
            }

            SetCanBeDeleted();
        }

        private async Task ChangeActiveAccount(int id)
        {
            _logger.Information(
                $"{nameof(ChangeActiveAccount)}: Changing current active user to userId = {id}");
            var userInDbResponse = await _dataService
                .UserService
                .FirstOrDefaultAsNoTrackingAsync(u => u.ID == id);

            if (!userInDbResponse.Succeed)
            {
                _dialogService.ShowSnackBar(userInDbResponse.Message);
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
                _dialogService.ShowSnackBar(
                    $"The user could not be saved / updated into the db. Error = {userSaveResponse.Message}");
                return;
            }

            _userCredentialService.UpdateUserCredential(
                ResourceType.LOGGED_USER_RESOURCE,
                _userCredentialService.DefaultUsername,
                false,
                userInDbResponse.Result.Email);

            bool isNetworkAvailable = _networkService.IsInternetAvailable();
            if (isNetworkAvailable && _appSettings.RunFullSyncAfterSwitchingAccounts)
            {
                var syncResults = new List<EmptyResponseDto>
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

                _dialogService.ShowSnackBar(message);
            }
            else if (!isNetworkAvailable)
            {
                _dialogService.ShowSnackBar(
                    "Internet is not available, a full sync will not be performed");
            }
            ChangeIsActiveStatus(false, id);
            Messenger.Publish(new ShowDrawerMsg(this, false));
            Messenger.Publish(new ActiveAccountChangedMsg(this));
            await _navigationService.Close(this);
        }

        private void ChangeIsActiveStatus(bool isActive, int? exceptId = null)
        {
            foreach (var account in Accounts)
            {
                if (exceptId.HasValue && account.Id == exceptId.Value)
                    account.IsActive = !isActive;
                else
                    account.IsActive = isActive;
            }
        }

        private void SetCanBeDeleted()
        {
            foreach (var account in Accounts)
            {
                account.CanBeDeleted = Accounts.Count == 1
                    ? false
                    : true;
            }
        }
    }
}