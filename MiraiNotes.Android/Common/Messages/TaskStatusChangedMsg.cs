using MiraiNotes.Core.Enums;
using MvvmCross.Plugin.Messenger;
using System;

namespace MiraiNotes.Android.Common.Messages
{
    public class TaskStatusChangedMsg : MvxMessage
    {
        public string TaskId { get; set; }
        public string ParentTask { get; set; }
        public bool HasParentTask
            => !string.IsNullOrEmpty(ParentTask);
        public DateTimeOffset? CompletedOn { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string NewStatus { get; set; }
        public TaskStatusChangedMsg(
            object sender,
            string taskId,
            string parentTask,
            DateTimeOffset? completedOn,
            DateTimeOffset updatedAt,
            string newStatus)
            : base(sender)
        {
            TaskId = taskId;
            ParentTask = parentTask;
            CompletedOn = completedOn;
            UpdatedAt = updatedAt;
            NewStatus = newStatus;
        }
    }
}