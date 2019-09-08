using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.ViewModels.Dialogs;
using MiraiNotes.Android.ViewModels.Settings;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Members

        private readonly IUserCredentialService _userCredentialService;
        private readonly IDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IMvxNavigationService _navigationService;

        private readonly MvxInteraction<bool> _showDrawer = new MvxInteraction<bool>();
        private readonly MvxInteraction<AppThemeChangedMsg> _appThemeChanged = new MvxInteraction<AppThemeChangedMsg>();
        private readonly MvxInteraction _hideKeyboard = new MvxInteraction();
        #endregion




        private string _language = "English";
        private MvxSubscriptionToken _cultureChangedToken;

        public string AppName => "Hola k ase";

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }


        #region Interactors
        public IMvxInteraction<bool> ShowDrawer
            => _showDrawer;

        public IMvxInteraction<AppThemeChangedMsg> AppThemeChanged
            => _appThemeChanged;

        public IMvxInteraction HideKeyboard
            => _hideKeyboard;
        #endregion

        #region Properties

        #endregion


        #region Commands
        public IMvxCommand ChangeLanguageCommand { get; private set; }
        public IMvxAsyncCommand OnSettingsSelectedCommand { get; private set; }
        public IMvxAsyncCommand OnAccountsSelectedCommand { get; private set; }
        public IMvxCommand LogoutCommand { get; private set; }
        public IMvxAsyncCommand InitViewCommand { get; private set; }
        #endregion

        public MainViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IDialogService dialogService,
            IUserCredentialService userCredentialService,
            IMiraiNotesDataService dataService,
            IAppSettingsService appSettings,
            IMvxNavigationService navigationService)
            : base(textProvider, messenger, appSettings)
        {
            _userCredentialService = userCredentialService;
            _dialogService = dialogService;
            _dataService = dataService;
            _navigationService = navigationService;
            _cultureChangedToken = Messenger.Subscribe<CultureChangedMessage>(ChangeCurrentLanguage);

            SetCommands();
            RegisterMessages();
        }

        private void SetCommands()
        {
            ChangeLanguageCommand = new MvxCommand(ChangeLanguage);
            OnSettingsSelectedCommand = new MvxAsyncCommand(
                async () => await _navigationService.Navigate<SettingsMainViewModel>());
            OnAccountsSelectedCommand = new MvxAsyncCommand(OnAccountsSelected);
            LogoutCommand = new MvxCommand(() =>
            {
                _dialogService.ShowDialog(
                "Confirmation",
                "Are you sure you want to log out?",
                "Yes",
                "No",
                async () => await Logout());
            });
            InitViewCommand = new MvxAsyncCommand(
                async () => await _navigationService.Navigate<MenuViewModel>());
        }

        private void RegisterMessages()
        {
            var tokens = new[]
            {
                Messenger.Subscribe<ShowDrawerMsg>(msg => _showDrawer.Raise(msg.Show)),
                Messenger.Subscribe<AppThemeChangedMsg>(msg => _appThemeChanged.Raise(msg)),
                Messenger.Subscribe<HideKeyboardMsg>(_ => _hideKeyboard.Raise())
            };

            SubscriptionTokens.AddRange(tokens);
        }

        public async Task Logout()
        {
            //TODO: DELETE ALL !!
            //delete all from the db
            //delete user settings
            //delete all view models
            //OpenPane(false);
            //ShowLoading(true, "Logging out... Please wait..");
            //BackgroundTasksManager.UnregisterBackgroundTask();
            AppSettings.ResetAppSettings();

            await _dataService
                .UserService
                .ChangeCurrentUserStatus(false);

            string currentLoggedUsername = _userCredentialService.GetCurrentLoggedUsername();
            if (string.IsNullOrEmpty(currentLoggedUsername))
                currentLoggedUsername = _userCredentialService.DefaultUsername;

            _userCredentialService.DeleteUserCredential(ResourceType.ALL, currentLoggedUsername);

            await _navigationService.Close(this);
            await _navigationService.Navigate<LoginViewModel>();
            //_navigationService.GoBack();
            //ShowLoading(false);
        }

        private async Task OnAccountsSelected()
        {
            Messenger.Publish(new ShowDrawerMsg(this, false));
            await _navigationService.Navigate<AccountDialogViewModel>();
        }




        public void ChangeLanguage()
        {
            string lang = "es";
            if (Language == "Español")
                lang = "en";
            else
                lang = "es";

            var msg = new LanguageChangedMsg(this, lang);
            Language = lang == "es" ? "Español" : "English";
            Messenger.Publish(msg);
        }

        public async void ChangeCurrentLanguage(CultureChangedMessage msg)
        {
            await RaiseAllPropertiesChanged();
            var testing = this["Testing"];
            var welcome = this["Welcome"];
        }
    }
}