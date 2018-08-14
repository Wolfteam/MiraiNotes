using MiraiNotes.UWP.Models;
using System;
using System.Collections.ObjectModel;
using Template10.Validation;

namespace MiraiNotes.UWP.ViewModels
{
    public class TaskItemViewModel : ValidatableModelBase
    {
        #region Members
        private bool _hasSubTasks;
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
                    return true;
                else
                    return false;
            }
            set
            {
                RaisePropertyChanged(nameof(CanBeMarkedAsCompleted));
            }
        }

        public bool CanBeMarkedAsIncompleted
        {
            get
            {
                if (!IsNew && TaskStatus == GoogleTaskStatus.COMPLETED)
                    return true;
                else
                    return false;
            }
            set
            {
                RaisePropertyChanged(nameof(CanBeMarkedAsIncompleted));
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

        public bool HasSubTasks
        {
            get
            {
                _hasSubTasks = SubTasks != null && SubTasks.Count > 0;
                return _hasSubTasks;
            }
            set
            {
                _hasSubTasks = value;
                RaisePropertyChanged(nameof(HasSubTasks));
            }
        }

        public ObservableCollection<TaskItemViewModel> SubTasks
        {
            get
            {
                var items = Read<ObservableCollection<TaskItemViewModel>>();
                if (items?.Count > 0)
                    HasSubTasks = true;
                else
                    HasSubTasks = false;
                return items;
            }
            set
            {
                Write(value);
                RaisePropertyChanged(nameof(SubTasks));
            }
        }

        public bool ShowSubTasks
        {
            get { return Read<bool>(); }
            set { Write(value); }
        }

        public bool HasParentTask
        {
            get => !string.IsNullOrEmpty(ParentTask);
        }
    }
}
