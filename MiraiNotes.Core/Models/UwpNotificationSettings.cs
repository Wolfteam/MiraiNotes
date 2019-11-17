namespace MiraiNotes.Core.Models
{
    public class UwpNotificationSettings
    {
        /// <summary>
        /// A tag for this toast. 
        /// If you are already showing a toast with this tag, it will be replaced by the new one
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Indicates if the toast shows a dismiss button
        /// </summary>
        public bool ShowDismissButton { get; set; } = true;

        /// <summary>
        /// If true, the toast will not generate a sound
        /// </summary>
        public bool IsAudioSilent { get; set; }
    }
}
