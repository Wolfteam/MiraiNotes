namespace MiraiNotes.Core.Enums
{
    /// <summary>
    /// This are the intervals that SyncBackgroudTask may run
    /// They are expressed in minutes
    /// </summary>
    public enum SyncBgTaskIntervals
    {
        NEVER = 0,
        EACH_30_MIN = 30,
        EACH_1_HOUR = 60,
        EACH_3_HOURS = 180,
        EACH_6_HOURS = 360,
        EACH_12_HOURS = 720,
        EACH_24_HOURS = 1440
    }
}
