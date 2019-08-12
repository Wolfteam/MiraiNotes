using System.Collections.Generic;
using MvvmCross.Localization;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;

namespace MiraiNotes.Android.ViewModels
{
    public class BaseViewModel : MvxViewModel
    {
        public IMvxMessenger Messenger { get; private set; }
        public IMvxLanguageBinder TextSource => new MvxLanguageBinder(string.Empty, string.Empty);

        private IMvxTextProvider _textProvider;
        
        public List<MvxSubscriptionToken> SubscriptionTokens = new List<MvxSubscriptionToken>();
        
        public BaseViewModel(IMvxTextProvider textProvider, IMvxMessenger messenger)
        {
            _textProvider = textProvider;
            Messenger = messenger;
        }

        public string this[string key] 
            => _textProvider.GetText(string.Empty, string.Empty, key);

        public override void ViewDestroy(bool viewFinishing = true)
        {
            base.ViewDestroy(viewFinishing);
            foreach (var token in SubscriptionTokens)
            {
                token.Dispose();
            }
        }
    }
}