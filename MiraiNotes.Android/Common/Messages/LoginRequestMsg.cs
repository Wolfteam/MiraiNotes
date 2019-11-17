using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class LoginRequestMsg : MvxMessage
    {
        public string Url { get; set; }

        public LoginRequestMsg(object sender, string url) : base(sender)
        {
            Url = url;
        }
    }
}