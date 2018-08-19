using MiraiNotes.Shared.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace MiraiNotes.Data.Models
{
    public class GoogleTask
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string GoogleTaskID { get; set; }

        [Required]
        public string Title { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        //[Required]
        //public string SelfLink { get; set; }

        public string ParentTask { get; set; }

        public string Position { get; set; }

        public string Notes { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTimeOffset? ToBeCompletedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsHidden { get; set; }

        public LocalStatus LocalStatus { get; set; }

        public bool ToBeSynced { get; set; }

        public virtual GoogleTaskList TaskList { get; set; }
    }
}
