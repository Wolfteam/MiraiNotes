using System;
using System.Collections.ObjectModel;
using MiraiNotes.Core.Enums;
using MiraiNotes.Shared.Helpers;
using Template10.Validation;

namespace MiraiNotes.UWP.ViewModels
{
    public class TaskItemViewModel : ValidatableModelBase
    {
        #region Members

        private string _taskID;
        private string _status;
        private string _parentTask;
        private bool _hasSubTasks;
        private DateTimeOffset? _completedOn;
        private ObservableCollection<TaskItemViewModel> _subTasks = new ObservableCollection<TaskItemViewModel>();
        private DateTimeOffset? _remindOn;

        #endregion

        public int Id { get; set; }

        public string TaskID
        {
            get => _taskID;
            set
            {
                _taskID = value;
                RaisePropertyChanged(nameof(TaskID));
                RaisePropertyChanged(nameof(IsNew));
            }
        }

        public string Title
        {
            get { return Read<string>(); }
            set { Write(value); }
        }

        public DateTimeOffset UpdatedAt
        {
            get { return Read<DateTimeOffset>(); }
            set { Write(value); }
        }

        public string SelfLink { get; set; }

        public string ParentTask
        {
            get => _parentTask;
            set
            {
                _parentTask = value;
                RaisePropertyChanged(nameof(ParentTask));
                RaisePropertyChanged(nameof(HasParentTask));
            }
        }

        public string Position { get; set; }

        public string Notes
        {
            get { return Read<string>(); }
            set { Write(value); }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                RaisePropertyChanged(nameof(Status));
                RaisePropertyChanged(nameof(CanBeMarkedAsCompleted));
                RaisePropertyChanged(nameof(CanBeMarkedAsIncompleted));
                RaisePropertyChanged(nameof(IsCompleted));
                //This is until uwp team fixes the text decoration not getting updated
                RaisePropertyChanged(nameof(Title));
            }
        }

        public GoogleTaskStatus TaskStatus
        {
            get
            {
                //when the task is new, it doesnt contain a status
                //until you save it to google tasks
                if (string.IsNullOrEmpty(Status))
                    return GoogleTaskStatus.NEEDS_ACTION;
                return GoogleTaskStatusHelper.GetGoogleStatus(Status);
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

        public DateTimeOffset? CompletedOn
        {
            get => _completedOn;
            set
            {
                _completedOn = value;
                RaisePropertyChanged(nameof(CompletedOn));
                RaisePropertyChanged(nameof(IsCompleted));
                RaisePropertyChanged(nameof(CompletitionDateText));
                RaisePropertyChanged(nameof(FullCompletitionDateText));
            }
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
            => string.IsNullOrEmpty(TaskID);

        public bool IsCompleted => CompletedOn != null && TaskStatus == GoogleTaskStatus.COMPLETED;

        public bool CanBeMarkedAsCompleted
        {
            get
            {
                if (!IsNew && TaskStatus != GoogleTaskStatus.COMPLETED)
                    return true;
                else
                    return false;
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
        }

        public bool IsSelected
        {
            get { return Read<bool>(); }
            set { Write(value); }
        }

        public bool IsCompletitionDateSet
            => ToBeCompletedOn.HasValue;

        public string CompletitionDateText
        {
            get
            {
                if (!ToBeCompletedOn.HasValue)
                    return string.Empty;
                var difference = DateTimeOffset.Now.Subtract(ToBeCompletedOn.Value).Days;
                if (difference >= 0)
                {
                    if (difference == 0)
                        return "Today";
                    else if (difference == 1)
                        return $"{difference} day ago";
                    else
                        return $"{difference} days ago";
                }
                else if (difference == -1)
                    return "Tomorrow";
                else
                    return $"{ToBeCompletedOn.Value:ddd, MMM d, yyyy}";
            }
        }

        public string FullCompletitionDateText
        {
            get
            {
                if (!ToBeCompletedOn.HasValue)
                    return string.Empty;
                var difference = DateTimeOffset.Now.Subtract(ToBeCompletedOn.Value).Days;
                if (difference >= 0)
                {
                    if (difference == 0)
                        return "This task is marked to be completed Today";
                    else
                        return $"This task was marked to be completed on {ToBeCompletedOn.Value:ddd, MMM d, yyyy}";
                }
                else if (difference == -1)
                    return "This task is marked to be completed Tomorrow";
                else
                    return $"This task is marked to be completed on {ToBeCompletedOn.Value:ddd, MMM d, yyyy}";
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
                if (_subTasks?.Count > 0)
                    HasSubTasks = true;
                else
                    HasSubTasks = false;
                return _subTasks;
            }
            set
            {
                _subTasks = value;
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

        //For some reason, google doesnt allows to create subtask on a completed task..
        public bool CanAddSubTasks
            => (!IsNew && !HasParentTask && CanBeMarkedAsCompleted) || (IsNew && !HasParentTask);

        public DateTimeOffset? RemindOn
        {
            get => _remindOn;
            set
            {
                _remindOn = value;
                RaisePropertyChanged(nameof(HasAReminderDate));
                RaisePropertyChanged(nameof(RemindOnDateText));
                RaisePropertyChanged(nameof(RemindOnTime));
            }
        }

        public string RemindOnGUID { get; set; }

        public TimeSpan? RemindOnTime
        {
            get => RemindOn?.TimeOfDay;
            set
            {
                if (_remindOn.HasValue)
                {
                    long timeDiff = DateTimeOffset.Now.TimeOfDay.Ticks - value.Value.Ticks;
                    if (timeDiff > 0)
                        value = DateTimeOffset.Now.TimeOfDay;

                    RemindOn = RemindOn.Value.Date + value;
                    RaisePropertyChanged(nameof(RemindOn));
                    RaisePropertyChanged(nameof(HasAReminderDate));
                    RaisePropertyChanged(nameof(RemindOnDateText));
                }
            }
        }

        public bool HasAReminderDate
            => RemindOn != null;

        public string RemindOnDateText =>
            !RemindOn.HasValue
            ? string.Empty
            : RemindOn.Value.ToString("ddd, MMM d HH:mm");
    }
}