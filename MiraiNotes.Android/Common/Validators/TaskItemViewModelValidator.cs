using FluentValidation;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.ViewModels;

namespace MiraiNotes.Android.Common.Validators
{
    public class TaskItemViewModelValidator : AbstractValidator<TaskItemViewModel>
    {
        private const int TitleMaxLength = 50;
        private const int NotesMaxLength = 4000;
        public TaskItemViewModelValidator(ITextProvider textProvider)
        {
            RuleFor(vm => vm.Title)
                .NotEmpty()
                .WithMessage(textProvider.Get("TitleCannotBeEmpty"))
                .MaximumLength(TitleMaxLength)
                .WithMessage(textProvider.Get("TitleMaxLength", $"{TitleMaxLength}" ));

            RuleFor(vm => vm.Notes)
                .NotEmpty()
                .WithMessage(textProvider.Get("NotesCannotBeEmpty"))
                .MaximumLength(NotesMaxLength)
                .WithMessage(textProvider.Get("NotesMaxLength", $"{NotesMaxLength}"));
        }
    }
}