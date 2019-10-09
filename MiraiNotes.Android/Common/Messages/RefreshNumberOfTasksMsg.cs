using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class RefreshNumberOfTasksMsg : MvxMessage
    {
        public bool WasAdded { get; }
        public string TaskListId { get; }
        public bool TaskWasMoved
            => !string.IsNullOrEmpty(TaskListId);

        public RefreshNumberOfTasksMsg(object sender, bool wasAdded) : base(sender)
        {
            WasAdded = wasAdded;
        }

        public RefreshNumberOfTasksMsg(object sender, bool wasAdded, string taskListId) : base(sender)
        {
            WasAdded = wasAdded;
            TaskListId = taskListId;
        }
    }
}