using MiraiNotes.Core.Enums;

namespace MiraiNotes.Android.Models
{
    public class NotificationAction
    {
        public NotificationActionType Action { get; set; }
        public string TaskListId { get; set; }
        public string TaskId { get; set; }
    }
}