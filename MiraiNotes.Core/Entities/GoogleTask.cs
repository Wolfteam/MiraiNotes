using System;
using System.ComponentModel.DataAnnotations;
using MiraiNotes.Core.Enums;

namespace MiraiNotes.Core.Entities
{
    public class GoogleTask
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string GoogleTaskID { get; set; }

        [Required]
        public string Title { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        //[Required]
        //public string SelfLink { get; set; }

        public string ParentTask { get; set; }

        public string Position { get; set; }

        public string Notes { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTimeOffset? ToBeCompletedOn { get; set; }

        public DateTimeOffset? CompletedOn { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsHidden { get; set; }

        public LocalStatus LocalStatus { get; set; }

        public bool ToBeSynced { get; set; }

        public DateTimeOffset? RemindOn { get; set; }

        public string RemindOnGUID { get; set; }


        [Required]
        public virtual GoogleTaskList TaskList { get; set; }
    }
}
