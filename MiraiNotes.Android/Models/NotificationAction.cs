using MiraiNotes.Core.Enums;

namespace MiraiNotes.Android.Models
{
    public class NotificationAction
    {
        public NotificationActionType Action { get; set; }
        public int TaskListId { get; set; }
        public int TaskId { get; set; }
    }
}