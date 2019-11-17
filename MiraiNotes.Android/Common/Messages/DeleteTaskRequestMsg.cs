using MiraiNotes.Android.ViewModels;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class DeleteTaskRequestMsg : MvxMessage
    {
        public TaskItemViewModel Task { get; }
        public DeleteTaskRequestMsg(object sender, TaskItemViewModel task) : base(sender)
        {
            Task = task;
        }
    }
}