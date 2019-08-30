using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Plugin.Messenger;
using Serilog;

namespace MiraiNotes.Android.ViewModels
{
    public class GoogleUserViewModel : BaseViewModel
    {
        #region Members
        private readonly IDialogService _dialogService;

        private int _id;
        private string _googleUserID;
        private string _fullName;
        private string _email;
        private bool _isActive;
        private bool _canBeDeleted;

        #endregion

        #region Properties

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string GoogleUserID
        {
            get => _googleUserID;
            set => SetProperty(ref _googleUserID, value);
        }

        public string Fullname
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string PictureUrl
            => MiscellaneousUtils.GetUserProfileImagePath(GoogleUserID);

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public bool CanBeDeleted
        {
            get => _canBeDeleted;
            set => SetProperty(ref _canBeDeleted, value);
        }
        #endregion

        #region Commands
        public IMvxCommand ChangeCurrentAccountCommand { get; private set; }
        public IMvxCommand DeleteAccountCommand { get; private set; }
        #endregion

        public GoogleUserViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IDialogService dialogService) 
            : base(textProvider, messenger)
        {
            _dialogService = dialogService;

            SetCommands();
        }

        private void SetCommands()
        {
            ChangeCurrentAccountCommand = new MvxCommand(() =>
                Messenger.Publish(new AccountChangeRequestMsg(this, false, true, this)));
            DeleteAccountCommand = new MvxCommand(() =>
                _dialogService.ShowDialog(
                    $"Delete {Fullname} ?",
                    "Are you sure you wanna delete this account ?",
                    "Yes",
                    "No",
                    () => Messenger.Publish(new AccountChangeRequestMsg(this, true, false, this))
                )
            );
        }
    }
}