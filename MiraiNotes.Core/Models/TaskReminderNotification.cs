using System;

namespace MiraiNotes.Core.Models
{
    public class TaskReminderNotification
    {
        public int Id { get; set; }
        public string TaskListId { get; set; }
        public string TaskId { get; set; }
        public string TaskListTitle { get; set; }
        public string TaskTitle { get; set; }
        public string TaskBody { get; set; }
        public DateTimeOffset DeliveryOn { get; set; }
    }
}
