using Android.App;
using Android.Content;
using Android.Net;
using MiraiNotes.Abstractions.Services;

namespace MiraiNotes.Android.Services
{
    public class NetworkService : INetworkService
    {
        public bool IsInternetAvailable()
        {
            var connectivityManager =
                (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
            if (connectivityManager?.ActiveNetwork == null)
                return false;
            var activeNetwork = connectivityManager.GetNetworkCapabilities(connectivityManager.ActiveNetwork);
            //TODO: CHECK THE ISCONNECTED 
            return activeNetwork != null &&
                   activeNetwork.HasCapability(NetCapability.Internet) &&
                   (activeNetwork.HasTransport(TransportType.Wifi) ||
                    activeNetwork.HasTransport(TransportType.Cellular) ||
                    activeNetwork.HasTransport(TransportType.Ethernet));
        }
    }
}