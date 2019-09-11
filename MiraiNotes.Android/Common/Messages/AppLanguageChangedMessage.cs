using MiraiNotes.Core.Enums;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android
{
    public class AppLanguageChangedMessage : MvxMessage
    {
        public AppLanguageType NewLanguage { get; }
        public AppLanguageChangedMessage(object sender, AppLanguageType appLanguage) : base(sender)
        {
            NewLanguage = appLanguage;
        }
    }
}