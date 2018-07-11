using System.Net.Http;

namespace MiraiNotes.UWP.Interfaces
{
    public interface IHttpClientsFactory
    {
        HttpClient GetHttpClient();
    }
}
