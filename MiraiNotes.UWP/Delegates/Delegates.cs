using MiraiNotes.UWP.Models;

namespace MiraiNotes.UWP.Delegates
{
    public delegate void ShowInAppNotificationRequest(string message);

    public delegate void SettingsNavigationRequest(SettingsPageType settingsPageType);

    public delegate void ChangeCurrentThemeRequest(AppThemeType appTheme);
}
