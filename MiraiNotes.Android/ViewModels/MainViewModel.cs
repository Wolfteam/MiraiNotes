using MvvmCross.Commands;
using MvvmCross.Localization;
using MvvmCross.Plugin.Messenger;
using System.Windows.Input;

namespace MiraiNotes.Android.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        //private readonly IMvxMessenger _messenger;
        private string _language = "English";
        private MvxSubscriptionToken _cultureChangedToken;

        public string AppName => "Hola k ase";
        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        public static bool IsDarkTheme;


        public ICommand ChangeLanguageCommand => new MvxCommand(ChangeLanguage);
        public ICommand ChangeThemeCommand => new MvxCommand(ChangeTheme);

        public MainViewModel(IMvxTextProvider textProvider, IMvxMessenger messenger)
            : base(textProvider, messenger)
        {
            _cultureChangedToken = Messenger.Subscribe<CultureChangedMessage>(ChangeCurrentLanguage);
        }
        
        


        public void ChangeLanguage()
        {
            string lang = "es";
            if (Language == "Español")
                lang = "en";
            else
                lang = "es";

            var msg = new LanguageChangedMsg(this, lang);
            Language = lang == "es" ? "Español" : "English";
            Messenger.Publish(msg);
        }

        public void ChangeTheme()
        {
            Messenger.Publish(new ChangeThemeMsg(this, !IsDarkTheme));
            IsDarkTheme = !IsDarkTheme;
        }

        public async void ChangeCurrentLanguage(CultureChangedMessage msg)
        {
            await RaiseAllPropertiesChanged();
            var testing = this["Testing"];
            var welcome = this["Welcome"];
        }
    }
}