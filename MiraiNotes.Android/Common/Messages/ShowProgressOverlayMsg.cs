using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class ShowProgressOverlayMsg : MvxMessage
    {
        public bool Show { get; }

        public ShowProgressOverlayMsg(object sender, bool show = true) 
            : base(sender)
        {
            Show = show;
        }
    }
}