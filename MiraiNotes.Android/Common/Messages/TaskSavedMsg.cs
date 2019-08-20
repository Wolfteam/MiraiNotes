using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class TaskSavedMsg : MvxMessage
    {
        public string TaskId { get; }
        public TaskSavedMsg(object sender, string taskId) : base(sender)
        {
            TaskId = taskId;
        }
    }
}