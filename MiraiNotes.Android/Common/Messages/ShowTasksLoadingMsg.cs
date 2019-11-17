using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class ShowTasksLoadingMsg : MvxMessage
    {
        public bool Show { get; private set; }
        public ShowTasksLoadingMsg(object sender, bool show = true)
            : base(sender)
        {
            Show = show;
        }
    }
}