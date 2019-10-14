using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class TaskSavedMsg : MvxMessage
    {
        public string TaskId { get; }
        public int ItemsAdded { get; }

        public TaskSavedMsg(object sender, string taskId, int itemsAdded = 1) : base(sender)
        {
            TaskId = taskId;
            ItemsAdded = itemsAdded;
        }
    }
}