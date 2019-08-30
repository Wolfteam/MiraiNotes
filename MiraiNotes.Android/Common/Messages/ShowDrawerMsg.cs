using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class ShowDrawerMsg : MvxMessage
    {
        public bool Show { get; private set; }

        public ShowDrawerMsg(object sender, bool show)
            : base(sender)
        {
            Show = show;
        }
    }
}