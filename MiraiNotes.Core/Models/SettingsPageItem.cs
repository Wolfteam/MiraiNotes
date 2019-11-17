using MiraiNotes.Core.Enums;

namespace MiraiNotes.Core.Models
{
    public class SettingsPageItem
    {
        public SettingsPageType PageType { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
    }
}
