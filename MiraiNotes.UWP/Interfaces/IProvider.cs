namespace MiraiNotes.UWP.Interfaces
{
    public interface IProvider
    {
        string ProviderName { get; }
        string AuthorizationEndpoint { get; }
        string TokenEndpoint { get; }
        string RedirectUrl { get; }
        string ApprovalUrl { get; }
    }
}
