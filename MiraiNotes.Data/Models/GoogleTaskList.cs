using MiraiNotes.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiraiNotes.Data.Models
{
    public class GoogleTaskList
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string GoogleTaskListID { get; set; }

        [Required]
        public string Title { get; set; }

        //[Required]
        //public string SelfLink { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public LocalStatus LocalStatus { get; set; }

        public bool ToBeSynced { get; set; }

        public virtual IEnumerable<GoogleTask> Tasks { get; set; }

        [Required]
        public virtual GoogleUser User { get; set; }
    }
}
