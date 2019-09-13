using MiraiNotes.Core.Enums;
using MvvmCross.Localization;

namespace MiraiNotes.Android.Interfaces
{
    public interface ITextProvider : IMvxTextProvider
    {
        string Get(string key);
        string Get(string key, params string[] formatArgs);
        void SetLanguage(AppLanguageType appLanguage, bool restartActivity = true);
    }
}