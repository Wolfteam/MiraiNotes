using FluentValidation;
using MiraiNotes.Android.ViewModels.Dialogs;

namespace MiraiNotes.Android.Common.Validators
{
    public class PasswordDialogViewModelValidator : AbstractValidator<PasswordDialogViewModel>
    {
        public PasswordDialogViewModelValidator()
        {
            When(vm => vm.PromptForPassword, () =>
            {
                RuleFor(vm => vm.Password)
                    .NotEmpty()
                    .WithMessage("Password cannot be empty")
                    .Equal(vm => vm.CurrentPassword)
                    .WithMessage("Password is incorrect");
            }).Otherwise(() =>
            {
                RuleFor(vm => vm.Password)
                    .NotEmpty()
                    .WithMessage("Password cannot be empty")
                    .MaximumLength(10)
                    .WithMessage("Password max length is 10")
                    .Equal(vm => vm.ConfirmPassword)
                    .WithMessage("Password does not match");

                RuleFor(vm => vm.ConfirmPassword)
                    .NotEmpty()
                    .WithMessage("Confirm password cannot be empty")
                    .MaximumLength(10)
                    .WithMessage("Confirm password max length is 10")
                    .Equal(vm => vm.Password)
                    .WithMessage("Confirm password does not match");
            });
        }
    }
}