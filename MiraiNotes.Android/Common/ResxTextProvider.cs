using MvvmCross;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Plugin.ResxLocalization;
using System.Globalization;
using System.Resources;

namespace MiraiNotes.Android
{
    public class ResxTextProvider : MvxResxTextProvider
    {
        private readonly ResourceManager _resourceManager;
        private readonly MvxSubscriptionToken _token;
        IMvxMessenger messenger;

        public ResxTextProvider(ResourceManager resourceManager) : base(resourceManager)
        {
            _resourceManager = resourceManager;

            messenger = Mvx.IoCProvider.Resolve<IMvxMessenger>();
            if (messenger != null)
            {
                _token = messenger.Subscribe<LanguageChangedMsg>(OnlanguageChange);
            }
        }

        private void OnlanguageChange(LanguageChangedMsg msg)
        {
            //Settings.ApplicationLanguage = code;
            //Strings.Culture = new CultureInfo(code);
            //CultureInfo.DefaultThreadCurrentUICulture = Strings.Culture;
            //((MvxResxTextProvider)textProvider).CurrentLanguage = Strings.Culture;

            //let all ViewModels that are active know the culture changed
            CurrentLanguage = new CultureInfo(msg.Language);
            messenger.Publish(new CultureChangedMessage(this));
        }
    }
}