namespace MiraiNotes.UWP.Interfaces
{
    public interface IApplicationSettingsServiceBase
    {
        object this[string key] { get; set; }
    }
}
