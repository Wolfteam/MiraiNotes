namespace MiraiNotes.Core.Models
{
    public class TaskNotification
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Conntent { get; set; }
        public UwpNotificationSettings UwpSettings { get; set; }
    }
}
