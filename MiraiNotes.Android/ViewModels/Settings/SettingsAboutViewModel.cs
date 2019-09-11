using System.Threading.Tasks;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsAboutViewModel : BaseViewModel
    {
        public string AppVersion
            => $"Version {MiscellaneousUtils.GetAppVersion()}";

        public SettingsAboutViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings)
            : base(textProvider, messenger, logger.ForContext<SettingsMainViewModel>(), navigationService, appSettings)
        {
        }

        public override Task Initialize()
        {
            return base.Initialize();
        }
    }
}