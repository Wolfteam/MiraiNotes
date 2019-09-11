using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class SettingsTitleChanged : MvxMessage
    {
        public string NewTitle { get; }
        public SettingsTitleChanged(object sender, string newtitle) : base(sender)
        {
            NewTitle = newtitle;
        }
    }
}