using MiraiNotes.Core.Enums;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android
{
    public class AppLanguageChangedMessage : MvxMessage
    {
        public AppLanguageType NewLanguage { get; }
        public bool RestartActivity { get;}
        public AppLanguageChangedMessage(
            object sender, 
            AppLanguageType appLanguage,
            bool restartActivity) : base(sender)
        {
            NewLanguage = appLanguage;
            RestartActivity = restartActivity;
        }
    }
}