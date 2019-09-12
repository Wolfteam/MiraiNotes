using FluentValidation;
using MiraiNotes.Android.ViewModels.Dialogs;

namespace MiraiNotes.Android.Common.Validators
{
    public class TaskListDialogViewModelValidator : AbstractValidator<TaskListDialogViewModel>
    {
        public TaskListDialogViewModelValidator()
        {
            RuleFor(vm => vm.TaskListTitle)
                .MaximumLength(50)
                .WithMessage("Title cannot have more than 50 chars")
                .NotEmpty()
                .WithMessage("Title cannot be empty");
        }
    }
}