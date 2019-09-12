using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class RefreshNumberOfTasksMsg : MvxMessage
    {
        public bool WasAdded { get; }
        public RefreshNumberOfTasksMsg(object sender, bool wasAdded) : base(sender)
        {
            WasAdded = wasAdded;
        }
    }
}