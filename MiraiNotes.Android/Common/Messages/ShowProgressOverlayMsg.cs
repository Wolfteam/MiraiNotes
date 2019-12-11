using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class ShowProgressOverlayMsg : MvxMessage
    {
        public bool Show { get; }

        public string Msg { get; set; }

        public ShowProgressOverlayMsg(object sender, bool show = true, string msg = null) 
            : base(sender)
        {
            Show = show;
            Msg = msg;
        }
    }
}