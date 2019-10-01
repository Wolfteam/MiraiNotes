using FluentValidation;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.ViewModels.Dialogs;
using System;

namespace MiraiNotes.Android.Common.Validators
{
    public class TaskReminderDialogViewModelValidator : AbstractValidator<TaskReminderDialogViewModel>
    {
        public TaskReminderDialogViewModelValidator(ITextProvider textProvider)
        {
            RuleFor(dto => dto.ReminderDateText)
                .NotEmpty()
                .WithMessage(textProvider.Get("ReminderDateCannotBeEmpty"));

            RuleFor(dto => dto.ReminderHourText)
                .NotEmpty()
                .WithMessage(textProvider.Get("ReminderHourCannotBeEmpty"))
                .Must((dto, hourText) =>
                {
                    var currentTime = DateTime.Now;
                    var selectedDate = DateTime.Parse(dto.FullReminderText);
                    return selectedDate > currentTime;
                })
                .WithMessage(textProvider.Get("InvalidReminderHour"));

            RuleFor(dto => dto.FullReminderText)
                .Must(text => DateTime.TryParse(text, out _))
                .WithMessage(textProvider.Get("InvalidReminderDate"));
        }
    }
}