using FluentValidation;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Extensions;
using MiraiNotes.Android.Common.Validators;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Core.Enums;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class PasswordDialogViewModel : BaseViewModel<bool, bool>
    {
        #region Members
        private readonly IUserCredentialService _credentialService;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IDialogService _dialogService;
        private readonly IValidator _validator;

        public string CurrentPassword;

        private bool _promptForPassword;
        private string _password;
        private string _confirmPassword;
        private ObservableDictionary<string, string> _errors = new ObservableDictionary<string, string>();
        #endregion

        #region Properties
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                Validate();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                Validate();
            }
        }

        public bool PromptForPassword
        {
            get => _promptForPassword;
            set => SetProperty(ref _promptForPassword, value);
        }

        public bool IsSaveButtonEnabled
            => Errors.Count == 0;

        public ObservableDictionary<string, string> Errors
        {
            get => _errors;
            set => SetProperty(ref _errors, value);
        }
        #endregion

        #region Commands
        public IMvxAsyncCommand SaveChangesCommand { get; private set; }
        public IMvxAsyncCommand CloseCommand { get; private set; }
        #endregion

        public PasswordDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IAppSettingsService appSettings,
            IMvxNavigationService navigationService,
            IDialogService dialogService,
            IUserCredentialService userCredentialService,
            IMiraiNotesDataService dataService)
            : base(textProvider, messenger, logger.ForContext<PasswordDialogViewModel>(), navigationService, appSettings)
        {
            _credentialService = userCredentialService;
            _dataService = dataService;
            _dialogService = dialogService;
            _validator = new PasswordDialogViewModelValidator();
            SetComamnds();
        }

        public override void Prepare(bool parameter)
        {
            PromptForPassword = parameter;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            Logger.Information($"{nameof(Initialize)}: Starting this vm with PromptForPassword = {PromptForPassword}");
            if (!PromptForPassword)
                return;

            var response = await _dataService.UserService.GetCurrentActiveUserAsync();
            CurrentPassword = _credentialService.GetUserCredential(
                ResourceType.SETTINGS_PASSWORD_RESOURCE,
                response.Result.Email);
        }

        private void SetComamnds()
        {
            SaveChangesCommand = new MvxAsyncCommand(SaveChanges);
            CloseCommand = new MvxAsyncCommand(async () =>
                await NavigationService.Close(this, false));
        }

        private async Task SaveChanges()
        {
            Validate();

            if (!IsSaveButtonEnabled)
                return;

            if (!PromptForPassword)
            {
                await SavePassword();
            }

            await NavigationService.Close(this, true);
        }

        private async Task SavePassword()
        {
            var response = await _dataService.UserService.GetCurrentActiveUserAsync();
            if (!response.Succeed || response.Result is null)
            {
                AppSettings.AskForPasswordWhenAppStarts = false;
                Logger.Error(
                    $"{nameof(SavePassword)}: Couldnt retrieve current active user. " +
                    $"Error = {response.Message}");
                _dialogService.ShowSnackBar(GetText("DatabaseUnknownError"));
                return;
            }

            _credentialService.DeleteUserCredential(
                ResourceType.SETTINGS_PASSWORD_RESOURCE,
                response.Result.Email);
            _credentialService.SaveUserCredential(
                ResourceType.SETTINGS_PASSWORD_RESOURCE,
                response.Result.Email,
                Password);
        }

        private void Validate()
        {
            var validationResult = _validator.Validate(this);
            var dictionary = validationResult.Errors
                .GroupBy(k => k.PropertyName)
                .ToDictionary(k => k.Key, v => v.First().ErrorMessage);
            Errors.Clear();
            Errors.AddRange(dictionary);

            RaisePropertyChanged(() => IsSaveButtonEnabled);
        }
    }
}