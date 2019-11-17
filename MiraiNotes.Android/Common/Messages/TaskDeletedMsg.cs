using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class TaskDeletedMsg : MvxMessage
    {
        public string TaskId { get; private set; }
        public string ParentTask { get; set; }
        public bool HasParentTask
            => !string.IsNullOrEmpty(ParentTask);


        public TaskDeletedMsg(
            object sender,
            string taskId,
            string parentTask = null) : base(sender)
        {
            TaskId = taskId;
            ParentTask = parentTask;
        }
    }
}