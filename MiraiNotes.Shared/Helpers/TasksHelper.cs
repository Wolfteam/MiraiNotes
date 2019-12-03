namespace MiraiNotes.Shared.Helpers
{
    public class TasksHelper
    {
        public static string GetNotesForNotification(string currentNotes)
            => currentNotes.Length > 15
                ? $"{currentNotes.Substring(0, 15)}...."
                : $"{currentNotes}....";

        public static bool HasReminderId(string remindOnGuid, out int id)
        {
            id = 0;

            if (string.IsNullOrEmpty(remindOnGuid))
                return false;

            return int.TryParse(remindOnGuid, out id);
        }
    }
}
