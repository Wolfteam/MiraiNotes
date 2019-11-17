using FluentValidation;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.ViewModels.Dialogs;
using MiraiNotes.Core.Enums;
using System;

namespace MiraiNotes.Android.Common.Validators
{
    public class TaskDateDialogViewModelValidator : AbstractValidator<TaskDateDialogViewModel>
    {
        public TaskDateDialogViewModelValidator(ITextProvider textProvider)
        {
            RuleFor(dto => dto.DateText)
                .NotEmpty()
                .WithMessage(textProvider.Get("ReminderDateCannotBeEmpty"));

            When(dto => dto.Parameter.DateType == TaskNotificationDateType.REMINDER_DATE, () =>
            {
                RuleFor(dto => dto.HourText)
                    .NotEmpty()
                    .WithMessage(textProvider.Get("ReminderHourCannotBeEmpty"))
                    .Must((dto, hourText) =>
                    {
                        var currentTime = DateTime.Now;
                        var selectedDate = DateTime.Parse(dto.FullText, textProvider.CurrentCulture);
                        return selectedDate > currentTime;
                    })
                    .WithMessage(textProvider.Get("InvalidReminderHour"));

                RuleFor(dto => dto.FullText)
                    .Must(text => DateTime.TryParse(text, out _))
                    .WithMessage(textProvider.Get("InvalidReminderDate"));
            });
        }
    }
}