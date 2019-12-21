using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class ChangeTasksSelectionModeMsg : MvxMessage
    {
        public bool IsInSelectionMode { get; }
        public ChangeTasksSelectionModeMsg(object sender, bool isInSelectionMode)
            : base(sender)
        {
            IsInSelectionMode = isInSelectionMode;
        }
    }
}