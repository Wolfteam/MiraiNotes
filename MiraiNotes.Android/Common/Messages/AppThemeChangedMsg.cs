using MiraiNotes.Core.Enums;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android
{
    public class AppThemeChangedMsg : MvxMessage
    {
        public AppThemeType AppTheme { get; }
        public string AccentColor { get; }
        public bool RestartActivity { get; }

        public AppThemeChangedMsg(object sender, AppThemeType appTheme, string accentColor, bool restartActivity = true) : base(sender)
        {
            AppTheme = appTheme;
            AccentColor = accentColor;
            RestartActivity = restartActivity;
        }
    }
}