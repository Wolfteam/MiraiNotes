using System;
using Template10.Validation;

namespace MiraiNotes.UWP.Models
{
    public class TaskModel : ValidatableModelBase
    {
        public string TaskID { get; set; }

        public string Title { get { return Read<string>(); } set { Write(value); } }

        public DateTime UpdatedAt { get; set; }

        public string SelfLink { get; set; }

        public string ParentTask { get; set; }

        public string Position { get; set; }

        public string Notes { get { return Read<string>(); } set { Write(value); } }

        public string Status { get; set; }

        public GoogleTaskStatus TaskStatus
        {
            get
            {
                string status = Status.ToUpper();
                Enum.TryParse(status, out GoogleTaskStatus googleTaskStatus);
                return googleTaskStatus;
            }
        }

        public DateTimeOffset? ToBeCompletedOn { get; set; }

        public DateTime? CompletedOn { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsHidden { get; set; }

        public bool IsNew { get; set; }
    }
}
