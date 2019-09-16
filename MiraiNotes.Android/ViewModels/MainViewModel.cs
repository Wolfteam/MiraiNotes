using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.ViewModels.Dialogs;
using MiraiNotes.Android.ViewModels.Settings;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Members
        private readonly IUserCredentialService _userCredentialService;
        private readonly IDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IBackgroundTaskManagerService _backgroundTaskManager;
        private readonly MvxInteraction<bool> _showDrawer = new MvxInteraction<bool>();
        private readonly MvxInteraction<AppThemeChangedMsg> _appThemeChanged = new MvxInteraction<AppThemeChangedMsg>();
        private readonly MvxInteraction _appLanguageChanged = new MvxInteraction();
        private readonly MvxInteraction _hideKeyboard = new MvxInteraction();
        #endregion

        #region Interactors
        public IMvxInteraction<bool> ShowDrawer
            => _showDrawer;

        public IMvxInteraction<AppThemeChangedMsg> AppThemeChanged
            => _appThemeChanged;

        public IMvxInteraction AppLanguageChanged
            => _appLanguageChanged;

        public IMvxInteraction HideKeyboard
            => _hideKeyboard;
        #endregion

        #region Properties

        #endregion


        #region Commands
        public IMvxAsyncCommand OnSettingsSelectedCommand { get; private set; }
        public IMvxAsyncCommand OnAccountsSelectedCommand { get; private set; }
        public IMvxCommand LogoutCommand { get; private set; }
        public IMvxAsyncCommand InitViewCommand { get; private set; }
        public IMvxCommand SyncCommand { get; private set; }
        #endregion

        public MainViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IDialogService dialogService,
            IUserCredentialService userCredentialService,
            IMiraiNotesDataService dataService,
            IAppSettingsService appSettings,
            IBackgroundTaskManagerService backgroundTaskManager)
            : base(textProvider, messenger, logger.ForContext<MainViewModel>(), navigationService, appSettings)
        {
            _userCredentialService = userCredentialService;
            _dialogService = dialogService;
            _dataService = dataService;
            _backgroundTaskManager = backgroundTaskManager;

            SetCommands();
            RegisterMessages();
        }

        private void SetCommands()
        {
            OnSettingsSelectedCommand = new MvxAsyncCommand(
                async () => await NavigationService.Navigate<SettingsMainViewModel>());
            OnAccountsSelectedCommand = new MvxAsyncCommand(OnAccountsSelected);
            LogoutCommand = new MvxCommand(() =>
            {
                _dialogService.ShowDialog(
                    GetText("Confirmation"),
                    GetText("WannaLogOut"),
                    GetText("Yes"),
                    GetText("No"),
                    async () => await Logout());
            });
            InitViewCommand = new MvxAsyncCommand(
                async () => await NavigationService.Navigate<MenuViewModel>());

            SyncCommand = new MvxCommand(
                () => _backgroundTaskManager.StartBackgroundTask(BackgroundTaskType.SYNC));
        }

        private void RegisterMessages()
        {
            var tokens = new[]
            {
                Messenger.Subscribe<ShowDrawerMsg>(msg => _showDrawer.Raise(msg.Show)),
                Messenger.Subscribe<AppThemeChangedMsg>(msg => _appThemeChanged.Raise(msg)),
                Messenger.Subscribe<AppLanguageChangedMessage>(msg => 
                {
                    if (msg.RestartActivity)
                        _appLanguageChanged.Raise();
                }),
                Messenger.Subscribe<HideKeyboardMsg>(_ => _hideKeyboard.Raise())
            };

            SubscriptionTokens.AddRange(tokens);
        }

        public async Task Logout()
        {
            //delete all from the db
            //delete user settings
            //delete all view models
            Messenger.Publish(new ShowProgressOverlayMsg(this));

            _backgroundTaskManager.UnregisterBackgroundTasks(BackgroundTaskType.ANY);
            AppSettings.ResetAppSettings();

            await _dataService
                .UserService
                .ChangeCurrentUserStatus(false);

            string currentLoggedUsername = _userCredentialService.GetCurrentLoggedUsername();
            if (string.IsNullOrEmpty(currentLoggedUsername))
                currentLoggedUsername = _userCredentialService.DefaultUsername;

            _userCredentialService.DeleteUserCredential(ResourceType.ALL, currentLoggedUsername);

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            await NavigationService.Close(this);
            await NavigationService.Navigate<LoginViewModel>();
        }

        private async Task OnAccountsSelected()
        {
            Messenger.Publish(new ShowDrawerMsg(this, false));
            await NavigationService.Navigate<AccountDialogViewModel>();
        }
    }
}