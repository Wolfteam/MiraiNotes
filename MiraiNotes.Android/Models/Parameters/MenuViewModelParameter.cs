namespace MiraiNotes.Android.Models.Parameters
{
    public class MenuViewModelParameter
    {
        public NotificationAction Notification { get; }
        private MenuViewModelParameter(NotificationAction notification)
        {
            Notification = notification;
        }

        public static MenuViewModelParameter Instance(NotificationAction notification)
        {
            return new MenuViewModelParameter(notification);
        }
    }
}