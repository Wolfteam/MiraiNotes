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
                (ConnectivityManager) Application.Context.GetSystemService(Context.ConnectivityService);
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            return activeNetworkInfo != null && activeNetworkInfo.IsConnected;
        }
    }
}