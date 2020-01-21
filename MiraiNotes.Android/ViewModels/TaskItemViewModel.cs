using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Android.Models.Results;
using MiraiNotes.Android.ViewModels.Dialogs;
using MiraiNotes.Core.Enums;
using MiraiNotes.Shared.Helpers;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using MvvmCross.ViewModels;
using Serilog;
using System;

namespace MiraiNotes.Android.ViewModels
{
    public class TaskItemViewModel : BaseViewModel
    {
        #region Members

        private string _googleId;
        private string _status;
        private string _notes;
        private string _parentTask;
        private bool _hasSubTasks;
        private bool _showSubTasks;
        private DateTimeOffset? _completedOn;
        private DateTimeOffset? _toBeCompletedOn;
        private MvxObservableCollection<TaskItemViewModel> _subTasks = new MvxObservableCollection<TaskItemViewModel>();
        private DateTimeOffset? _remindOn;
        private bool _isSelected;
        private bool _canBeSelected;
        #endregion

        #region Properties
        public int Id { get; set; }

        public string GoogleId
        {
            get => _googleId;
            set
            {
                SetProperty(ref _googleId, value);
                RaisePropertyChanged(nameof(IsNew));
            }
        }

        public new string Title { get; set; }

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
            get => _notes;
            set
            {
                SetProperty(ref _notes, value);
                RaisePropertyChanged(() => HasNotes);
            }
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
            get => _toBeCompletedOn;
            set
            {
                SetProperty(ref _toBeCompletedOn, value);
                RaisePropertyChanged(nameof(HasAToBeCompletedDate));
                RaisePropertyChanged(nameof(ToBeCompletedOnText));
                RaisePropertyChanged(nameof(FullToBeCompletedOnText));
            }
        }

        public DateTimeOffset? CompletedOn
        {
            get => _completedOn;
            set
            {
                SetProperty(ref _completedOn, value);
                RaisePropertyChanged(nameof(IsCompleted));
                RaisePropertyChanged(nameof(ToBeCompletedOnText));
                RaisePropertyChanged(nameof(FullToBeCompletedOnText));
            }
        }

        public bool IsDeleted { get; set; }

        public bool IsHidden { get; set; }

        public bool IsNew
            => string.IsNullOrEmpty(GoogleId);

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
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;
                SetProperty(ref _isSelected, value);
                if (!HasParentTask)
                {
                    foreach (var st in SubTasks)
                    {
                        st.IsSelected = value;
                    }
                }

                Messenger.Publish(new TaskSelectectionModeChangedMsg(this));
            }
        }

        public bool CanBeSelected
        {
            get => _canBeSelected;
            set
            {
                if (_canBeSelected == value)
                    return;
                SetProperty(ref _canBeSelected, value);
            }
        }

        public bool HasAToBeCompletedDate
            => ToBeCompletedOn.HasValue;

        public string ToBeCompletedOnText
        {
            get
            {
                if (!ToBeCompletedOn.HasValue)
                    return string.Empty;
                var difference = DateTimeOffset.Now.Subtract(ToBeCompletedOn.Value).Days;
                if (difference >= 0)
                {
                    if (difference == 0)
                        return GetText("Today");
                    else if (difference == 1)
                        return GetText("OneDayAgo");
                    else
                        return GetText("XDayAgo", $"{difference}");
                }
                else if (difference == -1)
                    return GetText("Tomorrow");
                else
                    return $"{ToBeCompletedOn.Value:ddd, MMM d, yyyy}";
            }
        }

        public string FullToBeCompletedOnText
        {
            get
            {
                if (!ToBeCompletedOn.HasValue)
                    return string.Empty;
                var difference = DateTimeOffset.Now.Subtract(ToBeCompletedOn.Value).Days;

                //today
                if (difference == 0)
                    return GetText("ToBeCompletedOnB", GetText("Today"));
                //n days ago
                else if (difference > 0)
                    return GetText("ToBeCompletedOnC", $"{ToBeCompletedOn.Value:ddd, MMM d, yyyy}");
                //tomorrow
                else if (difference == -1)
                    return GetText("ToBeCompletedOnB", GetText("Tomorrow"));
                //in n days
                else
                    return GetText("ToBeCompletedOnA", $"{ToBeCompletedOn.Value:ddd, MMM d, yyyy}");
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
                SetProperty(ref _hasSubTasks, value);
            }
        }

        public MvxObservableCollection<TaskItemViewModel> SubTasks
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
                SetProperty(ref _subTasks, value);
            }
        }

        public bool ShowSubTasks
        {
            get => _showSubTasks;
            set => SetProperty(ref _showSubTasks, value);
        }

        public bool HasParentTask
        {
            get => !string.IsNullOrEmpty(ParentTask);
        }

        public DateTimeOffset? RemindOn
        {
            get => _remindOn;
            set
            {
                SetProperty(ref _remindOn, value);
                RaisePropertyChanged(nameof(HasAReminderDate));
                RaisePropertyChanged(nameof(RemindOnDateText));
                RaisePropertyChanged(nameof(RemindOnTime));
                RaisePropertyChanged(nameof(FullRemindOnText));
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
                    RaisePropertyChanged(nameof(FullRemindOnText));
                }
            }
        }

        public bool HasAReminderDate
            => RemindOn != null;

        public string FullRemindOnText =>
            !RemindOn.HasValue
                ? string.Empty
                : GetText("Reminder") + ": " + RemindOn.Value.ToString("ddd, MMM d HH:mm", TextProvider.CurrentCulture);

        public string RemindOnDateText =>
            !RemindOn.HasValue
                ? string.Empty
                : RemindOn.Value.ToString("ddd, MMM d HH:mm");

        public bool HasNotes
            => !string.IsNullOrEmpty(Notes);

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset UpdatedAt { get; set; }
        #endregion

        #region Commands
        public IMvxCommand ShowSubTasksCommand { get; private set; }
        public IMvxCommand<TaskItemViewModel> SubTaskSelectedCommand { get; private set; }
        public IMvxAsyncCommand DeleteSubTaskCommand { get; private set; }
        public IMvxCommand<TaskItemViewModel> ShowMenuOptionsDialogCommand { get; private set; }
        #endregion

        public TaskItemViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            ITelemetryService telemetryService)
            : base(textProvider, messenger, logger.ForContext<TaskItemViewModel>(), navigationService, appSettings, telemetryService)
        {
        }

        public override void SetCommands()
        {
            base.SetCommands();
            ShowSubTasksCommand = new MvxCommand(() => ShowSubTasks = !ShowSubTasks);

            SubTaskSelectedCommand = new MvxCommand<TaskItemViewModel>(
                (subTask) => Messenger.Publish(new SubTaskSelectedMsg(this, subTask)));

            DeleteSubTaskCommand = new MvxAsyncCommand(async() =>
            {
                var parameter = DeleteTaskDialogViewModelParameter.Delete(this);
                await NavigationService.Navigate<
                    DeleteTaskDialogViewModel,
                    DeleteTaskDialogViewModelParameter,
                    DeleteTaskDialogViewModelResult>(parameter);
            });

            ShowMenuOptionsDialogCommand = new MvxCommand<TaskItemViewModel>(
                (subTask) => Messenger.Publish(new SubTaskSelectedMsg(this, subTask, true)));
        }

        public void SubTaskWasRemoved()
        {
            RaisePropertyChanged(() => HasSubTasks);
        }
    }
}