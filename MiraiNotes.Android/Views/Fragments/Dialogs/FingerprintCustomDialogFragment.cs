using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Enums;
using MvvmCross;
using Plugin.Fingerprint.Dialog;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    public class FingerprintCustomDialogFragment : FingerprintDialogFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);
            var imageView = view.FindViewById<ImageView>(Resource.Id.fingerprint_imgFingerprint);

            var appSettings = Mvx.IoCProvider.Resolve<IAppSettingsService>();
            if (appSettings.AppTheme == AppThemeType.LIGHT)
            {
                imageView.SetColorFilter(Color.Gray);
            }
            else
            {
                imageView.SetColorFilter(Color.White);
            }
            return view;
        }
    }
}