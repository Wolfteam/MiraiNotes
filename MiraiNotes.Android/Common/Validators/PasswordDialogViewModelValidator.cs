using FluentValidation;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.ViewModels.Dialogs;

namespace MiraiNotes.Android.Common.Validators
{
    public class PasswordDialogViewModelValidator : AbstractValidator<PasswordDialogViewModel>
    {
        public PasswordDialogViewModelValidator(ITextProvider textProvider)
        {
            When(vm => vm.PromptForPassword, () =>
            {
                RuleFor(vm => vm.Password)
                    .NotEmpty()
                    .WithMessage(textProvider.Get("PasswordCantBeEmpty"))
                    .Equal(vm => vm.CurrentPassword)
                    .WithMessage(textProvider.Get("PasswordCantBeEmpty"));
            }).Otherwise(() =>
            {
                RuleFor(vm => vm.Password)
                    .NotEmpty()
                    .WithMessage(textProvider.Get("PasswordCantBeEmpty"))
                    .MaximumLength(10)
                    .WithMessage(textProvider.Get("PasswordMaxLength", "10"))
                    .Equal(vm => vm.ConfirmPassword)
                    .WithMessage(textProvider.Get("PasswordDoesntMatch"));

                RuleFor(vm => vm.ConfirmPassword)
                    .NotEmpty()
                    .WithMessage(textProvider.Get("ConfirmPasswordCantBeEmpty"))
                    .MaximumLength(10)
                    .WithMessage(textProvider.Get("ConfirmPasswordMaxLength", "10"))
                    .Equal(vm => vm.Password)
                    .WithMessage(textProvider.Get("ConfirmPasswordDoesntMatch"));
            });
        }
    }
}