using MiraiNotes.Android.ViewModels;
using MiraiNotes.Core.Enums;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class ChangeTaskStatusRequestMsg : MvxMessage
    {
        public TaskItemViewModel Task { get; }
        public GoogleTaskStatus NewStatus { get; }

        public ChangeTaskStatusRequestMsg(
            object sender,
            TaskItemViewModel task,
            GoogleTaskStatus newStatus = GoogleTaskStatus.COMPLETED) 
            : base(sender)
        {
            Task = task;
            NewStatus = newStatus;
        }
    }
}