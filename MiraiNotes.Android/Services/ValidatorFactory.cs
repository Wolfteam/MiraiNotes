using FluentValidation;
using MvvmCross;
using System;

namespace MiraiNotes.Android.Services
{
    public class ValidatorFactory : ValidatorFactoryBase
    {
        public override IValidator CreateInstance(Type validatorType)
        {
            return Mvx.IoCProvider.Resolve(validatorType) as IValidator;
        }
    }
}