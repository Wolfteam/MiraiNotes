using MvvmCross.Localization;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.ViewModels.Settings
{
    public class SettingsAboutViewModel : BaseViewModel
    {
        public SettingsAboutViewModel(
            IMvxTextProvider textProvider,
            IMvxMessenger messenger)
            : base(textProvider, messenger)
        {
        }
    }
}