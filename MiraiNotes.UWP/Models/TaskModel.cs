using System;
using Template10.Validation;

namespace MiraiNotes.UWP.Models
{
    public class TaskModel : ValidatableModelBase
    {
        #region Members
        private bool _canBeMarkedAsCompleted;
        #endregion


        public string TaskID { get; set; }

        public string Title
        {
            get { return Read<string>(); }
            set { Write(value); }
        }

        public DateTime UpdatedAt
        {
            get { return Read<DateTime>(); }
            set { Write(value); }
        }

        public string SelfLink { get; set; }

        public string ParentTask { get; set; }

        public string Position { get; set; }

        public string Notes
        {
            get { return Read<string>(); }
            set { Write(value); }
        }

        public string Status { get; set; }

        public GoogleTaskStatus TaskStatus
        {
            get
            {
                //when the task is new, it doesnt contain a status
                //until you save it to google tasks
                if (string.IsNullOrEmpty(Status))
                    return GoogleTaskStatus.NEEDS_ACTION;
                string status = Status.ToUpper();
                Enum.TryParse(status, out GoogleTaskStatus googleTaskStatus);
                return googleTaskStatus;
            }
        }

        public DateTimeOffset? ToBeCompletedOn
        {
            get { return Read<DateTimeOffset?>(); }
            set { Write(value); }
        }

        public DateTime? CompletedOn
        {
            get { return Read<DateTime?>(); }
            set { Write(value); }
        }

        public bool IsDeleted
        {
            get { return Read<bool>(); }
            set { Write(value); }
        }

        public bool IsHidden
        {
            get { return Read<bool>(); }
            set { Write(value); }
        }

        public bool IsNew
        {
            get { return Read<bool>(); }
            set { Write(value); }
        }

        public bool CanBeMarkedAsCompleted
        {
            get
            {
                if (!IsNew && TaskStatus != GoogleTaskStatus.COMPLETED)
                {
                    _canBeMarkedAsCompleted = true;

                }
                else
                {
                    _canBeMarkedAsCompleted = false;
                }
                return _canBeMarkedAsCompleted;
            }
            set
            {
                _canBeMarkedAsCompleted = value;
                RaisePropertyChanged(nameof(CanBeMarkedAsCompleted));
            }
        }

        public bool IsSelected
        {
            get { return Read<bool>(); }
            set { Write(value); }
        }
    }
}
