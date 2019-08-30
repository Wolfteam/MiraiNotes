using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class ActiveAccountChangedMsg : MvxMessage
    {
        public ActiveAccountChangedMsg(object sender) : base(sender)
        {
        }
    }
}