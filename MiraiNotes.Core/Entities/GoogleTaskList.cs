using MiraiNotes.Core.Enums;
using System;
using System.Collections.Generic;

namespace MiraiNotes.Core.Entities
{
    public class GoogleTaskList
    {
        public int ID { get; set; }

        public string GoogleTaskListID { get; set; }

        public string Title { get; set; }

        //[Required]
        //public string SelfLink { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        public LocalStatus LocalStatus { get; set; }

        public bool ToBeSynced { get; set; }

        public IEnumerable<GoogleTask> Tasks { get; set; } = new List<GoogleTask>();

        public int UserID { get; set; }
        public GoogleUser User { get; set; }
    }
}
