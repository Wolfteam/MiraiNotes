using AndroidX.Core.App;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Models;

namespace MiraiNotes.Android.Interfaces
{
    public interface IAndroidNotificationService : INotificationService
    {
        void ShowTaskReminderNotification(TaskReminderNotification notification);
        NotificationCompat.Builder BuildSimpleNotification(string title, string content, bool soundMuted = false);
    }
}