using FluentValidation;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.ViewModels.Dialogs;

namespace MiraiNotes.Android.Common.Validators
{
    public class AddEditTaskListDialogViewModelValidator : AbstractValidator<AddEditTaskListDialogViewModel>
    {
        public AddEditTaskListDialogViewModelValidator(ITextProvider textProvider)
        {
            RuleFor(vm => vm.TaskListTitle)
                .MaximumLength(50)
                .WithMessage(textProvider.Get("TitleMaxLength", "50"))
                .NotEmpty()
                .WithMessage(textProvider.Get("TitleCannotBeEmpty"));
        }
    }
}