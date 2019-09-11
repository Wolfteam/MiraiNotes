using MiraiNotes.Core.Enums;
using MvvmCross.Localization;

namespace MiraiNotes.Android.Interfaces
{
    public interface ITextProvider : IMvxTextProvider
    {
        void SetLanguage(AppLanguageType appLanguage, bool restartActivity = true);
    }
}