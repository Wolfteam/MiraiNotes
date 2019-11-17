using FluentValidation.Results;
using System.Collections.Generic;
using System.Linq;

namespace MiraiNotes.Android.Common.Utils
{
    public static class ValidationExtensions
    {
        public static Dictionary<string, string> ToDictionary(this ValidationResult validationResult)
        {
            return validationResult.Errors
                .GroupBy(k => k.PropertyName)
                .ToDictionary(k => k.Key, v => v.First().ErrorMessage);
        }
    }
}