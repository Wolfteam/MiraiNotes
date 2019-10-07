using MiraiNotes.Core.Enums;
using System;

namespace MiraiNotes.Core.Entities
{
    public class GoogleTask
    {
        public int ID { get; set; }

        public string GoogleTaskID { get; set; }

        public string Title { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        //[Required]
        //public string SelfLink { get; set; }

        public string ParentTask { get; set; }

        public string Position { get; set; }

        public string Notes { get; set; }

        public string Status { get; set; }

        public DateTimeOffset? ToBeCompletedOn { get; set; }

        public DateTimeOffset? CompletedOn { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsHidden { get; set; }

        public LocalStatus LocalStatus { get; set; }

        public bool ToBeSynced { get; set; }

        public DateTimeOffset? RemindOn { get; set; }

        public string RemindOnGUID { get; set; }

        public int TaskListID { get; set; }
        public GoogleTaskList TaskList { get; set; }
    }
}
