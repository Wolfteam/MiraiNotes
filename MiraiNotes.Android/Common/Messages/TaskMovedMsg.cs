namespace MiraiNotes.Android.Common.Messages
{
    public class TaskMovedMsg : TaskDeletedMsg
    {
        public string NewTaskListId { get; }
        public TaskMovedMsg(
            object sender, 
            string taskId, 
            string newTaskListId, 
            string parentTask = null) : base(sender, taskId, parentTask)
        {
            NewTaskListId = newTaskListId;
        }
    }
}