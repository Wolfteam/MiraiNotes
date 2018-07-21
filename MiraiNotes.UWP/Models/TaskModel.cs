using GalaSoft.MvvmLight;
using System;
using Template10.Validation;

namespace MiraiNotes.UWP.Models
{
    public class TaskModel : ViewModelBase
    {
        #region Members
        private string _title;
        private DateTime updatedAt;
        private string _notes;
        private DateTimeOffset? _toBeCompletedOn;
        private DateTime? _completedOn;
        private bool _isDeleted;
        private bool _isHidden;
        private bool _isNew;
        private bool _canBeMarkedAsCompleted;
        #endregion


        public string TaskID { get; set; }

        public string Title
        {
            get { return _title; }
            set { Set(ref _title, value); }
        }

        public DateTime UpdatedAt
        {
            get { return updatedAt; }
            set { Set(ref updatedAt, value); }
        }

        public string SelfLink { get; set; }

        public string ParentTask { get; set; }

        public string Position { get; set; }

        public string Notes
        {
            get { return _notes; }
            set { Set(ref _notes, value); }
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
            get { return _toBeCompletedOn; }
            set { Set(ref _toBeCompletedOn, value); }
        }

        public DateTime? CompletedOn
        {
            get { return _completedOn; }
            set { Set(ref _completedOn, value); }
        }

        public bool IsDeleted
        {
            get { return _isDeleted; }
            set { Set(ref _isDeleted, value); }
        }

        public bool IsHidden
        {
            get { return _isHidden; }
            set { Set(ref _isHidden, value); }
        }

        public bool IsNew
        {
            get { return _isNew; }
            set { Set(ref _isNew, value); }
        }

        public bool CanBeMarkedAsCompleted
        {
            get
            {
                if (!IsNew && TaskStatus != GoogleTaskStatus.COMPLETED)
                    _canBeMarkedAsCompleted = true;
                else
                    _canBeMarkedAsCompleted = false;
                return _canBeMarkedAsCompleted;
            }
            set
            {
                Set(ref _canBeMarkedAsCompleted, value);
            }
        }
    }
}
