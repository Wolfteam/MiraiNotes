namespace MiraiNotes.Android.Models.Parameters
{
    public class MenuViewModelParameter
    {
        public bool OrientationChanged { get;}
        public NotificationAction Notification { get; }
        private MenuViewModelParameter(NotificationAction notification, bool orientationChanged)
        {
            Notification = notification;
            OrientationChanged = orientationChanged;
        }

        public static MenuViewModelParameter Instance(NotificationAction notification, bool orientationChanged)
        {
            return new MenuViewModelParameter(notification, orientationChanged);
        }
    }
}