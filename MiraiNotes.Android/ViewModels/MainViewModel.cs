using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Members

        private readonly IUserCredentialService _userCredentialService;
        private readonly IDialogService _dialogService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IAppSettingsService _appSettings;
        private readonly IMvxNavigationService _navigationService;
        #endregion


        //private readonly IMvxMessenger _messenger;
        private string _language = "English";
        private MvxSubscriptionToken _cultureChangedToken;

        public string AppName => "Hola k ase";

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        public static bool IsDarkTheme;

        public IMvxCommand ChangeLanguageCommand { get; private set; }
        public IMvxCommand ChangeThemeCommand { get; private set; }
        public IMvxCommand OnSettingsSelectedCommand { get; private set; }
        public IMvxCommand OnAccountsSelectedCommand { get; private set; }
        public IMvxCommand LogoutCommand { get; private set; }
        public IMvxCommand InitViewCommand { get; private set; }

        public MainViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IDialogService dialogService,
            IUserCredentialService userCredentialService,
            IMiraiNotesDataService dataService,
            IAppSettingsService appSettings,
            IMvxNavigationService navigationService)
            : base(textProvider, messenger)
        {
            _userCredentialService = userCredentialService;
            _dialogService = dialogService;
            _dataService = dataService;
            _appSettings = appSettings;
            _navigationService = navigationService;
            _cultureChangedToken = Messenger.Subscribe<CultureChangedMessage>(ChangeCurrentLanguage);

            SetCommands();
        }

        private void SetCommands()
        {
            ChangeLanguageCommand = new MvxCommand(ChangeLanguage);
            ChangeThemeCommand = new MvxCommand(ChangeTheme);
            OnSettingsSelectedCommand = new MvxCommand(() => _dialogService.ShowWarningToast("Settings is not implemented"));
            OnAccountsSelectedCommand = new MvxCommand(() => _dialogService.ShowWarningToast("Accounts is not implemented"));
            LogoutCommand = new MvxCommand(() =>
            {
                _dialogService.ShowDialog(
                "Confirmation",
                "Are you sure you want to log out?",
                "Yes",
                "No",
                async () => await Logout());
            });
            InitViewCommand = new MvxAsyncCommand(async () => await _navigationService.Navigate<MenuViewModel>());
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
            _appSettings.ResetAppSettings();

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

        public void ChangeTheme()
        {
            Messenger.Publish(new ChangeThemeMsg(this, !IsDarkTheme));
            IsDarkTheme = !IsDarkTheme;
        }

        public async void ChangeCurrentLanguage(CultureChangedMessage msg)
        {
            await RaiseAllPropertiesChanged();
            var testing = this["Testing"];
            var welcome = this["Welcome"];
        }
    }
}