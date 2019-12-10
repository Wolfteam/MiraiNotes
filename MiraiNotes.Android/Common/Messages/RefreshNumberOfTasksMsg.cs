using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android.Common.Messages
{
    public class RefreshNumberOfTasksMsg : MvxMessage
    {
        public bool UpdateCurrentTaskList { get; }
        public bool WasAdded { get; }
        public int AffectedItems { get; }
        public string MovedToTaskListId { get; }
        public bool TaskWasMoved
            => !string.IsNullOrEmpty(MovedToTaskListId);

        public RefreshNumberOfTasksMsg(object sender, bool wasAdded, int affectedItems, bool updateCurrentTaskList = true) 
            : base(sender)
        {
            WasAdded = wasAdded;
            AffectedItems = affectedItems;
            UpdateCurrentTaskList = updateCurrentTaskList;
        }

        public RefreshNumberOfTasksMsg(object sender, int affectedItems, string movedToTaskListId, bool updateCurrentTaskList = true) 
            : base(sender)
        {
            AffectedItems = affectedItems;
            MovedToTaskListId = movedToTaskListId;
            UpdateCurrentTaskList = updateCurrentTaskList;
        }
    }
}