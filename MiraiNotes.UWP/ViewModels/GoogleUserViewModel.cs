using GalaSoft.MvvmLight;
using MiraiNotes.UWP.Utils;

namespace MiraiNotes.UWP.ViewModels
{
    public class GoogleUserViewModel : ViewModelBase
    {
        private bool _isActive;

        public int ID { get; set; }
        public string GoogleUserID { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string PictureUrl
            => MiscellaneousUtils.GetUserProfileImagePath(GoogleUserID);
        public bool IsActive
        {
            get => _isActive;
            set => Set(ref _isActive, value);
        }
    }
}
