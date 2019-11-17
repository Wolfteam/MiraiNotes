using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Messages
{
    public class AuthCodeGrantedMsg : MvxMessage
    {
        public string AuthCode { get; set; }
        public AuthCodeGrantedMsg(object sender, string authCode) : base(sender)
        {
            AuthCode = authCode;
        }
    }
}