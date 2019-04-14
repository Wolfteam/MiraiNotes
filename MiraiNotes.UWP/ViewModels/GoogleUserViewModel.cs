using GalaSoft.MvvmLight;

namespace MiraiNotes.UWP.ViewModels
{
    public class GoogleUserViewModel : ViewModelBase
    {
        private bool _isActive;

        public int ID { get; set; }
        public string GoogleUserID { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string PictureUrl { get; set; }
        public bool IsActive
        {
            get => _isActive;
            set => Set(ref _isActive, value);
        }
    }
}
