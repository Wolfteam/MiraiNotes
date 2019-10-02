using FluentValidation;
using MiraiNotes.Android.Interfaces;
using MiraiNotes.Android.ViewModels.Dialogs;

namespace MiraiNotes.Android.Common.Validators
{
    public class AddSubTaskDialogViewModelValidator : AbstractValidator<AddSubTaskDialogViewModel>
    {
        public const int MaxChars = 255;
        public AddSubTaskDialogViewModelValidator(ITextProvider textProvider)
        {
            RuleFor(vm => vm.SubTaskTitle)
                .NotEmpty()
                .WithMessage(textProvider.Get("TitleCannotBeEmpty"))
                .MaximumLength(MaxChars)
                .WithMessage(textProvider.Get("TitleMaxLength", $"{MaxChars}"));
        }
    }
}