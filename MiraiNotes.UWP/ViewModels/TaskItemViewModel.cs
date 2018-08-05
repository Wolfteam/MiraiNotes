using MiraiNotes.UWP.Models;
using System;
using Template10.Validation;

namespace MiraiNotes.UWP.ViewModels
{
    public class TaskItemViewModel : ValidatableModelBase
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
            set
            {
                Write(value);
                RaisePropertyChanged(nameof(ToBeCompletedOn));
            }
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

        public bool IsCompletitionDateTodayOrAlreadyPassed
        {
            get
            {
                return ToBeCompletedOn?.Date <= DateTimeOffset.Now;
            }
        }

        public string CompletitionDateTodayOrAlreadyPassedText
        {
            get
            {
                if (!ToBeCompletedOn.HasValue)
                    return string.Empty;
                else
                {
                    var difference = DateTimeOffset.Now.DayOfYear - ToBeCompletedOn.Value.DayOfYear;
                    if (difference == 0)
                        return "Today";
                    else if (difference == 1)
                        return $"{difference} day ago";
                    else
                        return $"{difference} days ago";
                }
            }
        }
    }
}
