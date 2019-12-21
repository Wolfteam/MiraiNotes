using MiraiNotes.Core.Enums;
using MvvmCross.Plugin.Messenger;

namespace MiraiNotes.Android
{
    public class AppThemeChangedMsg : MvxMessage
    {
        public AppThemeType AppTheme { get; }
        public string AccentColor { get; }
        public bool RestartActivity { get; }
        public bool UseDarkAmoledTheme { get; }

        public AppThemeChangedMsg(
            object sender, 
            AppThemeType appTheme, 
            string accentColor, 
            bool useDarkAmoledTheme,
            bool restartActivity = true) : base(sender)
        {
            AppTheme = appTheme;
            AccentColor = accentColor;
            UseDarkAmoledTheme = useDarkAmoledTheme;
            RestartActivity = restartActivity;
        }
    }
}