using MiraiNotes.Android.ViewModels;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class TaskDateUpdatedMsg : MvxMessage
    {
        public bool IsAReminderDate { get; }
        public TaskItemViewModel Task { get; set; }

        public TaskDateUpdatedMsg(
            object sender,
            TaskItemViewModel task,
            bool isAReminderDate)
            : base(sender)
        {
            Task = task;
            IsAReminderDate = isAReminderDate;
        }
    }
}