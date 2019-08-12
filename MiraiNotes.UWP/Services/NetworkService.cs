using Microsoft.Toolkit.Uwp.Connectivity;
using MiraiNotes.Abstractions.Services;

namespace MiraiNotes.UWP.Services
{
    public class NetworkService : INetworkService
    {
        public bool IsInternetAvailable()
        {
            return NetworkHelper
                .Instance
                .ConnectionInformation
                .IsInternetAvailable;
        }
    }
}
