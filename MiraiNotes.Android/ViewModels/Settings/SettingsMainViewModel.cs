using System.Threading.Tasks;
using MiraiNotes.Abstractions.Services;
using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsMainViewModel : BaseViewModel
    {
        private readonly IMvxNavigationService _navigationService;

        public IMvxAsyncCommand InitViewCommand { get; private set; }

        public SettingsMainViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings)
            : base(textProvider, messenger, appSettings)
        {
            _navigationService = navigationService;

            SetCommands();
        }

        private void SetCommands()
        {
            InitViewCommand = new MvxAsyncCommand
                (async () => await _navigationService.Navigate<SettingsHomeViewModel>());
        }
    }
}