using GalaSoft.MvvmLight;

namespace MiraiNotes.UWP.ViewModels
{
    public class GoogleUserViewModel : ViewModelBase
    {
        #region Members
        private string _userID;
        private string _fullName;
        private string _email;
        private string _pictureUrl;
        #endregion

        #region Properties
        public string UserID
        {
            get => _userID;
            set => Set(ref _userID, value);
        }

        public string Fullname
        {
            get => _fullName;
            set => Set(ref _fullName, value);
        }

        public string Email
        {
            get => _userID;
            set => Set(ref _email, value);
        }

        public string PictureUrl
        {
            get => _pictureUrl;
            set => Set(ref _pictureUrl, value);
        }
        #endregion
    }
}
