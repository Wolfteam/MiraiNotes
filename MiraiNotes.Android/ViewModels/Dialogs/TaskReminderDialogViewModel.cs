using FluentValidation;
using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Android.Common.Extensions;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.Models.Parameters;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Plugin.Messenger;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Android.ViewModels.Dialogs
{
    public class TaskReminderDialogViewModel : BaseConfirmationDialogViewModel<TaskReminderDialogViewModelParameter, bool>
    {
        private string _reminderDate;
        private string _reminderHour;
        private readonly IMiraiNotesDataService _dataService;
        private readonly IValidator _validator;
        private readonly IDialogService _dialogService;
        private readonly INotificationService _notificationService;
        private ObservableDictionary<string, string> _errors = new ObservableDictionary<string, string>();

        public DateTime MinDate
            => DateTime.Now.AddMinutes(5);

        public string CurrentReminderText
        {
            get
            {
                if (!Parameter.Task.HasAReminderDate)
                    return string.Empty;

                string date = Parameter.Task.RemindOn.Value.ToString("f", TextProvider.CurrentCulture);
                string text = GetText("ReminderDateIsSetTo", date);
                return text;
            }
        }

        public string ReminderDateText
        {
            get => _reminderDate;
            set
            {
                SetProperty(ref _reminderDate, value);
                Validate();
            }
        }

        public string ReminderHourText
        {
            get => _reminderHour;
            set
            {
                SetProperty(ref _reminderHour, value);
                Validate();
            }
        }

        public string FullReminderText
            => $"{ReminderDateText} {ReminderHourText}";

        public bool IsSaveButtonEnabled
            => Errors.Count == 0;

        public ObservableDictionary<string, string> Errors
        {
            get => _errors;
            set => SetProperty(ref _errors, value);
        }
        //TODO: SHOULD I ALLOW DELETE / CREATE COMPLETETION DATES IN THIS VIEWMODEL?
        public IMvxAsyncCommand DeleteCurrentReminderCommand { get; set; }

        public TaskReminderDialogViewModel(
            ITextProvider textProvider,
            IMvxMessenger messenger,
            ILogger logger,
            IMvxNavigationService navigationService,
            IAppSettingsService appSettings,
            IMiraiNotesDataService dataService,
            IValidatorFactory validatorFactory,
            IDialogService dialogService,
            INotificationService notificationService)
            : base(textProvider, messenger, logger.ForContext<TaskReminderDialogViewModel>(), navigationService, appSettings)
        {
            _dataService = dataService;
            _validator = validatorFactory.GetValidator<TaskReminderDialogViewModel>();
            _dialogService = dialogService;
            _notificationService = notificationService;
        }

        public override void Prepare(TaskReminderDialogViewModelParameter parameter)
        {
            base.Prepare(parameter);
            Title = GetText("Confirmation");
            OkText = GetText("Ok");
            CancelText = GetText("Cancel");

            var date = Parameter.Task.HasAReminderDate
                ? Parameter.Task.RemindOn.Value.DateTime
                : MinDate;

            SetReminderDateText(date);
            SetReminderHourTexxt(date);

            //If this task already had a reminder, we hide the validation msg
            Errors.Clear();
            RaisePropertyChanged(() => IsSaveButtonEnabled);
        }

        public override void SetCommands()
        {
            base.SetCommands();
            OkCommand = new MvxAsyncCommand(AddReminderDate);
            CloseCommand = new MvxAsyncCommand(() => NavigationService.Close(this, false));
            DeleteCurrentReminderCommand = new MvxAsyncCommand(() => RemoveNotificationDate(TaskNotificationDateType.REMINDER_DATE));
        }

        public void SetReminderDateText(DateTime date)
            => ReminderDateText = date.ToString("D", TextProvider.CurrentCulture);

        public void SetReminderHourTexxt(DateTime date)
            => ReminderHourText = date.ToString("t", TextProvider.CurrentCulture);

        private async Task AddReminderDate()
        {
            Validate();

            if (!IsSaveButtonEnabled)
                return;

            Messenger.Publish(new ShowProgressOverlayMsg(this));

            var taskList = Parameter.TaskList;
            var task = Parameter.Task;
            var date = DateTime.Parse(FullReminderText);
            string guid = string.IsNullOrEmpty(task.RemindOnGUID)
                ? string.Join("", $"{task.GetHashCode()}".Where(c => c != '-'))
                : task.RemindOnGUID;

            var response = await _dataService.TaskService
                .AddNotificationDate(task.TaskID, TaskNotificationDateType.REMINDER_DATE, date, guid);

            if (!response.Succeed)
            {
                Logger.Error(
                    $"{nameof(AddReminderDate)} An error occurred while trying to " +
                    $"update taskId = {task.ID} into db. Error = {response.Message}");
            }
            else
            {
                task.RemindOn = response.Result.RemindOn;
                task.RemindOnGUID = response.Result.RemindOnGUID;

                string notes = task.Notes.Length > 15
                    ? $"{task.Notes.Substring(0, 15)}...."
                    : $"{task.Notes}....";

                int id = int.Parse(guid);
                _notificationService.RemoveScheduledNotification(id);
                _notificationService.ScheduleNotification(new TaskReminderNotification
                {
                    Id = id,
                    TaskListId = taskList.Id,
                    TaskId = task.TaskID,
                    TaskListTitle = taskList.Title,
                    TaskTitle = task.Title,
                    TaskBody = notes,
                    DeliveryOn = date
                });
            }

            Messenger.Publish(new ShowProgressOverlayMsg(this, false));

            string msg = response.Succeed
                ? GetText("ReminderWasCreated")
                : GetText("DatabaseUnknownError");

            _dialogService.ShowSnackBar(msg);
            await NavigationService.Close(this, true);
        }

        private async Task RemoveNotificationDate(TaskNotificationDateType dateType)
        {
            Messenger.Publish(new ShowProgressOverlayMsg(this));

            string message = dateType == TaskNotificationDateType.TO_BE_COMPLETED_DATE
                ? GetText("Completition")
                : GetText("Reminder");

            var task = Parameter.Task;
            if (!task.IsNew)
            {
                if (dateType == TaskNotificationDateType.REMINDER_DATE &&
                    int.TryParse(task.RemindOnGUID, out int id))
                {
                    _notificationService.RemoveScheduledNotification(id);
                }

                var response = await _dataService
                    .TaskService
                    .RemoveNotificationDate(task.TaskID, dateType);

                if (!response.Succeed)
                {
                    Logger.Error(
                        $"{nameof(RemoveNotificationDate)}: Could not remove the {message} date of {task.Title}" +
                        $"Error = {response.Message}");
                    _dialogService.ShowErrorToast(GetText("DatabaseUnknownError"));
                    Messenger.Publish(new ShowProgressOverlayMsg(this, false));
                    return;
                }

                task.RemindOn = response.Result.RemindOn;
                task.RemindOnGUID = response.Result.RemindOnGUID;
                task.ToBeCompletedOn = response.Result.ToBeCompletedOn;
            }
            else
            {
                switch (dateType)
                {
                    case TaskNotificationDateType.TO_BE_COMPLETED_DATE:
                        task.ToBeCompletedOn = null;
                        break;
                    case TaskNotificationDateType.REMINDER_DATE:
                        task.RemindOn = null;
                        break;
                }
            }

            await RaisePropertyChanged(() => CurrentReminderText);
            Messenger.Publish(new ShowProgressOverlayMsg(this, false));
        }

        private void Validate()
        {
            Errors.Clear();
            var validationResult = _validator.Validate(this);
            Errors.AddRange(validationResult.ToDictionary());
            RaisePropertyChanged(() => IsSaveButtonEnabled);
        }
    }
}