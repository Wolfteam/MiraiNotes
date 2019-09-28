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
        private readonly IMapper _mapper;
        private readonly IDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly INetworkService _networkService;
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
            ITextProvider textProvider,
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
            : base(textProvider, messenger, logger.ForContext<AccountDialogViewModel>(), navigationService, appSettings)
        {
            _mapper = mapper;
            _dialogService = dialogService;
            _dataService = dataService;
            _networkService = networkService;
            _syncService = syncService;
            _googleApiService = googleApiService;
            _userCredentialService = userCredentialService;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            await InitView();
        }

        public override void SetCommands()
        {
            base.SetCommands();
            NewAccountCommand = new MvxCommand(AddAccountRequest);
            CloseCommand = new MvxAsyncCommand(async () => await NavigationService.Close(this));
            OnAuthCodeGrantedCommand = new MvxAsyncCommand<string>(OnCodeGranted);
        }

        public override void RegisterMessages()
        {
            base.RegisterMessages();
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
                Logger.Error(
                    $"{nameof(InitView)}: An error occurred while trying to get all the user accounts. " +
                    $"Error = {dbResponse.Message}");
                _dialogService.ShowErrorToast($"{GetText("UnknownErrorOccurred")}.");
                return;
            }

            Accounts = _mapper.Map<MvxObservableCollection<GoogleUserViewModel>>(dbResponse.Result);

            SetCanBeDeleted();
        }

        public void AddAccountRequest()
        {
            if (!_networkService.IsInternetAvailable())
            {
                _dialogService.ShowSnackBar(GetText("NetworkNotAvailable"));
                return;
            }
            var url = _googleApiService.GetAuthorizationUrl();
            _onAddAccountRequest.Raise(url);
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
                    Logger.Error(
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
                Logger.Error(ex,
                    $"{nameof(OnCodeGranted)}: An unknown error occurred while signing in the app");
                _dialogService.ShowSnackBar($"{GetText("UnknownErrorOccurred")}. Error = {ex.Message}");
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
            Logger.Information(
                $"{nameof(SignIn)}: Sign in the app started. Trying to get the user info from google");

            var userResponse = await _googleApiService.GetUser();
            if (!userResponse.Succeed)
            {
                Logger.Error($"{nameof(SignIn)}: Couldnt get the google user. Error = {userResponse.Message}");
                _dialogService.ShowSnackBar(GetText("UserNotFound"));
                return null;
            }

            var user = userResponse.Result;

            var userExistsResponse = await _dataService
                .UserService
                .ExistsAsync(u => u.GoogleUserID == user.ID);

            if (!userExistsResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(SignIn)}: Couldnt check if userId = {user.ID} exists in db. " +
                    $"Error = {userExistsResponse.Message}");
                _dialogService.ShowSnackBar(GetText("DatabaseUnknownError"));
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
                Logger.Information($"{nameof(SignIn)}: User doesnt exist in db. Creating a new one...");
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
                Logger.Information($"{nameof(SignIn)}: User exist in db. Updating it...");
                var userInDbResponse = await _dataService
                    .UserService
                    .FirstOrDefaultAsNoTrackingAsync(u => u.GoogleUserID == user.ID);

                if (!userInDbResponse.Succeed)
                {
                    Logger.Error($"{nameof(SignIn)}: Couldnt retrieve user in db. Error = {userInDbResponse.Message}");
                    _dialogService.ShowSnackBar(GetText("DatabaseUnknownError"));
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
                Logger.Error($"{nameof(SignIn)}: Couldnt save / update user in db. Error = {userSaveResponse.Message}");
                _dialogService.ShowSnackBar($"{GetText("DatabaseUnknownError")}.");
                return null;
            }

            Logger.Information(
                $"{nameof(SignIn)}: Trying to get all the task lists that are remote...");
            var syncResult = await _syncService.SyncDownTaskListsAsync(false);

            if (!syncResult.Succeed)
            {
                Logger.Error($"{nameof(SignIn)}: Couldnt sync down tasklists. Error = {syncResult.Message}");
                _dialogService.ShowSnackBar(GetText("SyncUnknownError"));
                return null;
            }

            Logger.Information(
                $"{nameof(SignIn)}: Trying to get all the tasks that are remote...");
            syncResult = await _syncService.SyncDownTasksAsync(false);

            if (!syncResult.Succeed)
            {
                Logger.Error($"{nameof(SignIn)}: Couldnt sync down tasks. Error = {syncResult.Message}");
                _dialogService.ShowSnackBar(GetText("SyncUnknownError"));
                return null;
            }

            Logger.Information($"{nameof(SignIn)}: Saving user credentials...");

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
            Logger.Information(
                $"{nameof(DeleteAccount)}: Deleting user = {account.Email}...");

            var userInDbResponse = await _dataService
                .UserService
                .FirstOrDefaultAsNoTrackingAsync(u => u.ID == account.Id);

            if (!userInDbResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(DeleteAccount)}: An error occurred wilhe trying to get account = {account.Email}..." +
                    $"Error = {userInDbResponse.Message}");
                _dialogService.ShowSnackBar(GetText("DatabaseUnknownError"));
                return;
            }

            var user = userInDbResponse.Result;

            if (user.IsActive && Accounts.Count == 1)
            {
                Logger.Warning($"{nameof(DeleteAccount)}: Default account cannot be deleted");
                _dialogService.ShowSnackBar(GetText("DefaultAccountCantBeDeleted"));
                return;
            }

            if (user.IsActive)
            {
                Logger.Information(
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

                Logger.Information(
                    $"{nameof(DeleteAccount)}: User = {defaultAccount.Email} will be marked as active");

                var response = await _dataService
                    .UserService
                    .SetAsCurrentUser(defaultAccount.Email);

                if (!response.Succeed)
                {
                    Logger.Error(
                        $"{nameof(DeleteAccount)}: Couldnt set {defaultAccount.Email} " +
                        $"as default. Error = {response.Message}");
                    _dialogService.ShowSnackBar(GetText("DatabaseUnknownError"));
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
            Logger.Information($"{nameof(DeleteAccount)}: Updating credentials...");

            _userCredentialService.DeleteUserCredential(
                ResourceType.REFRESH_TOKEN_RESOURCE,
                user.Email);

            _userCredentialService.DeleteUserCredential(
                ResourceType.TOKEN_RESOURCE,
                user.Email);

            var deleteResponse = await _dataService.UserService.RemoveAsync(account.Id);
            if (deleteResponse.Succeed)
            {
                Logger.Information($"{nameof(DeleteAccount)}: User was succesfully deleted");
                Accounts.Remove(account);
            }
            else
            {
                Logger.Error(
                    $"{nameof(DeleteAccount)}: User couldnt be deleted from db. " +
                    $"Error = {deleteResponse.Message}");
                _dialogService.ShowSnackBar(GetText("DatabaseUnknownError"));
            }

            SetCanBeDeleted();
        }

        private async Task ChangeActiveAccount(int id)
        {
            Logger.Information(
                $"{nameof(ChangeActiveAccount)}: Changing current active user to userId = {id}");

            Messenger.Publish(new ShowProgressOverlayMsg(this));
            var userInDbResponse = await _dataService
                .UserService
                .FirstOrDefaultAsNoTrackingAsync(u => u.ID == id);

            if (!userInDbResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(ChangeActiveAccount)}: Couldnt get the userId = {id} from db." +
                    $"Error = {userInDbResponse.Message}");
                _dialogService.ShowSnackBar(GetText("DatabaseUnknownError"));
                Messenger.Publish(new ShowProgressOverlayMsg(this, false));
                return;
            }

            var currentActiveUserResponse = await _dataService
                .UserService
                .ChangeCurrentUserStatus(false);

            if (!currentActiveUserResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(ChangeActiveAccount)}: Couldnt change the the status of the current active user");
                _dialogService.ShowSnackBar(GetText("DatabaseUnknownError"));
                Messenger.Publish(new ShowProgressOverlayMsg(this, false));
                return;
            }

            userInDbResponse.Result.IsActive = true;

            var userSaveResponse = await _dataService
                .UserService
                .UpdateAsync(userInDbResponse.Result);

            if (!userSaveResponse.Succeed)
            {
                Logger.Error(
                    $"{nameof(ChangeActiveAccount)}: UserId = {id}  couldnt be saved / updated." +
                    $"Error = {userSaveResponse.Message}");
                _dialogService.ShowSnackBar(GetText("DatabaseUnknownError"));
                Messenger.Publish(new ShowProgressOverlayMsg(this, false));
                return;
            }

            _userCredentialService.UpdateUserCredential(
                ResourceType.LOGGED_USER_RESOURCE,
                _userCredentialService.DefaultUsername,
                false,
                userInDbResponse.Result.Email);

            IsDialogVisible = false;

            bool isNetworkAvailable = _networkService.IsInternetAvailable();
            if (isNetworkAvailable && AppSettings.RunFullSyncAfterSwitchingAccounts)
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
                    : GetText("AutomaticSyncWasPerformed");

                if (string.IsNullOrEmpty(message))
                {
                    Logger.Error("An unknown sync error occurred");
                    message = GetText("SyncUnknownError");
                }

                _dialogService.ShowSnackBar(message);
            }
            else if (!isNetworkAvailable)
            {
                _dialogService.ShowSnackBar(GetText("NetworkNotAvailableForSync"));
            }

            IsDialogVisible = true;
            ChangeIsActiveStatus(false, id);
            Messenger.Publish(new ShowProgressOverlayMsg(this, false));
            Messenger.Publish(new ShowDrawerMsg(this, false));
            Messenger.Publish(new ActiveAccountChangedMsg(this));
            await NavigationService.Close(this);
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