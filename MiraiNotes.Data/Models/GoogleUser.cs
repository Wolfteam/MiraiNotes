using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MiraiNotes.Data.Models
{
    public class GoogleUser
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public string GoogleUserID { get; set; }

        [Required]
        public string Fullname { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string PictureUrl { get; set; }

        public bool IsActive { get; set; }

        public virtual IEnumerable<GoogleTaskList> TaskLists { get; set; }
    }
}
