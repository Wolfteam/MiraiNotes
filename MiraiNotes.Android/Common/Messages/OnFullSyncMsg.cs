using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class OnFullSyncMsg : MvxMessage
    {
        public bool FullSync { get; }
        public OnFullSyncMsg(object sender, bool fullSync = true) : base(sender)
        {
            FullSync = fullSync;
        }
    }
}