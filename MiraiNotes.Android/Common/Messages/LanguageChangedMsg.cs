using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android
{
    public class LanguageChangedMsg : MvxMessage
    {
        public LanguageChangedMsg(object sender, string lang) : base(sender)
        {
            Language = lang;
        }

        public string Language { get; set; }
    }
}