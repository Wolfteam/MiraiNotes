using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class RefreshNumberOfTasksMsg : MvxMessage
    {
        public bool WasAdded { get; }
        public int AffectedItems { get; }
        public string MovedToTaskListId { get; }
        public bool TaskWasMoved
            => !string.IsNullOrEmpty(MovedToTaskListId);

        public RefreshNumberOfTasksMsg(object sender, bool wasAdded, int affectedItems) : base(sender)
        {
            WasAdded = wasAdded;
            AffectedItems = affectedItems;
        }

        public RefreshNumberOfTasksMsg(object sender, int affectedItems, string movedToTaskListId) : base(sender)
        {
            AffectedItems = affectedItems;
            MovedToTaskListId = movedToTaskListId;
        }
    }
}