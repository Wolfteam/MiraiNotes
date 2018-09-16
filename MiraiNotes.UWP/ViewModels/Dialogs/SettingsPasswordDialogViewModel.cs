using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MiraiNotes.DataService.Interfaces;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MiraiNotes.UWP.ViewModels.Dialogs
{
    public class SettingsPasswordDialogViewModel : ViewModelBase
    {
        #region Members
        private readonly IApplicationSettingsService _appSettings;
        private readonly ICustomDialogService _dialogService;
        private readonly IUserCredentialService _credentialService;
        private readonly IMiraiNotesDataService _dataService;

        private bool _isSaveButtonEnabled;
        private string _password;
        private string _confirmPassword; 
        #endregion


        #region Properties
        public bool IsSaveButtonEnabled
        {
            get => _isSaveButtonEnabled;
            set => Set(ref _isSaveButtonEnabled, value);
        }

        public string Password
        {
            get => _password;
            set
            {
                Set(ref _password, value);
                CheckMatchingPassword();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                Set(ref _confirmPassword, value);
                CheckMatchingPassword();
            }
        } 
        #endregion


        #region Commands
        public ICommand LoadedCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand CloseCommand { get; set; } 
        #endregion


        public SettingsPasswordDialogViewModel(
            IApplicationSettingsService appSettings,
            ICustomDialogService dialogService,
            IUserCredentialService credentialService, 
            IMiraiNotesDataService dataService)
        {
            _appSettings = appSettings;
            _dialogService = dialogService;
            _dataService = dataService;
            _credentialService = credentialService;

            LoadedCommand = new RelayCommand(CleanInputs);
            SaveCommand = new RelayCommand
                (async () => await SavePassword());
            CloseCommand = new RelayCommand(CleanInputs);
        }


        #region Methods
        private async Task SavePassword()
        {
            var response = await _dataService.UserService.GetCurrentActiveUserAsync();
            if (!response.Succeed || response.Result is null)
            {
                _appSettings.AskForPasswordWhenAppStarts = false;
                await _dialogService.ShowMessageDialogAsync("Error", "Could not retrieve the current active user");
                return;
            }

            _credentialService.DeleteUserCredentials(
                PasswordVaultResourceType.SETTINGS_PASSWORD_RESOURCE, 
                response.Result.Email);
            _credentialService.SaveUserCredentials(
                PasswordVaultResourceType.SETTINGS_PASSWORD_RESOURCE,
                response.Result.Email,
                Password);
        }

        private void CleanInputs()
        {
            Password = ConfirmPassword = string.Empty;
        }

        private void CheckMatchingPassword()
        {
            if (!string.IsNullOrEmpty(Password) &&
                !string.IsNullOrEmpty(ConfirmPassword) &&
                Password == ConfirmPassword)
            {
                IsSaveButtonEnabled = true;
            }
            else
                IsSaveButtonEnabled = false;
        } 
        #endregion
    }
}
