﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Threading.Tasks;
using System.Windows.Input;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Enums;

namespace MiraiNotes.UWP.ViewModels.Dialogs
{
    public class LoginPasswordDialogViewModel : ViewModelBase
    {
        #region Members
        private readonly IUserCredentialService _credentialService;
        private readonly IMiraiNotesDataService _dataService;

        private string _password;
        private bool _isErrorVisible;
        #endregion

        #region Properties
        public string Password
        {
            get { return _password; }
            set { Set(ref _password, value); }
        }

        public bool IsErrorVisible
        {
            get { return _isErrorVisible; }
            set { Set(ref _isErrorVisible, value); }
        }
        #endregion

        #region Commands
        public ICommand LoadedCommand { get; set; }
        #endregion

        public LoginPasswordDialogViewModel(IUserCredentialService credentialService, IMiraiNotesDataService dataService)
        {
            _credentialService = credentialService;
            _dataService = dataService;

            LoadedCommand = new RelayCommand(() => Password = string.Empty);
        }

        public async Task<bool> PasswordMatches()
        {
            var response = await _dataService.UserService.GetCurrentActiveUserAsync();
            string currentPassowrd = _credentialService.GetUserCredential(
                ResourceType.SETTINGS_PASSWORD_RESOURCE,
                response.Result.Email);

            if (currentPassowrd == Password)
            {
                IsErrorVisible = false;
                return true;
            }
            IsErrorVisible = true;
            return false;
        }
    }
}
