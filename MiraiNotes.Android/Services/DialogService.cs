using Android.App;
using Android.Widget;

namespace MiraiNotes.Android.Services
{
    public class DialogService
    {
        public void ShowInfoToast(string msg)
        {
            Toast.MakeText(Application.Context, msg, ToastLength.Long).Show();
        }
    }
}