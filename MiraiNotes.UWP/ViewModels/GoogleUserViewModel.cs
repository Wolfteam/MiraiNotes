using GalaSoft.MvvmLight;
using MiraiNotes.UWP.Interfaces;

namespace MiraiNotes.UWP.ViewModels
{
    public class GoogleUserViewModel : ViewModelBase
    {
        private readonly IGoogleUserService _googleUserService;
        private bool _isActive;

        public int ID { get; set; }
        public string GoogleUserID { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string PictureUrl
            => _googleUserService.GetUserProfileImagePath(GoogleUserID);
        public bool IsActive
        {
            get => _isActive;
            set => Set(ref _isActive, value);
        }

        public GoogleUserViewModel(IGoogleUserService googleUserService)
        {
            _googleUserService = googleUserService;
        }
    }
}
