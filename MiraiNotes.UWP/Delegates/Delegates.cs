using MiraiNotes.Core.Enums;

namespace MiraiNotes.UWP.Delegates
{
    public delegate void ShowInAppNotificationRequest(string message);

    public delegate void SettingsNavigationRequest(SettingsPageType settingsPageType);

    public delegate void HideDialog();
}
