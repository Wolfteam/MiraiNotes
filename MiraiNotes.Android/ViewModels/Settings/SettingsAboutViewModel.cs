using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Utils;
using MvvmCross.Localization;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsAboutViewModel : BaseViewModel
    {
        public string AppVersion
            => $"Version {MiscellaneousUtils.GetAppVersion()}" ;

        public SettingsAboutViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger,
            IAppSettingsService appSettings)
            : base(textProvider, messenger, appSettings)
        {
        }
    }
}