using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiraiNotes.Core.Entities
{
    public class GoogleUser
    {
        public int ID { get; set; }

        public string GoogleUserID { get; set; }

        public string Fullname { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        public string PictureUrl { get; set; }

        public bool IsActive { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }

        //If you need to add tasklist from the user entity
        //you need to changes this to a icollection
        public IEnumerable<GoogleTaskList> TaskLists { get; set; } = new List<GoogleTaskList>();
    }
}
