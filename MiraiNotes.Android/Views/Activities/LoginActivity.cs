using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using MiraiNotes.Android.Common.Messages;
using MiraiNotes.Android.ViewModels;
using MvvmCross.Droid.Support.V7.AppCompat;
using AndroidUri = Android.Net.Uri;

namespace MiraiNotes.Android.Views.Activities
{
    [Activity(Label = "@string/app_name", NoHistory = true, LaunchMode = LaunchMode.SingleTask)]
    [IntentFilter(
        actions: new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataSchemes = new[] { "com.miraisoft.notes" }
    )]
    public class LoginActivity : MvxAppCompatActivity<LoginViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Login);

            var loginMsg = ViewModel.Messenger.Subscribe<LoginRequestMsg>(msg => Login(msg.Url));
            ViewModel.SubscriptionTokens.Add(loginMsg);
        }

        protected override void OnResume()
        {
            base.OnResume();

            var data = Intent.Data;
            var code = data?.GetQueryParameter("code");
            if (data == null || string.IsNullOrEmpty(data.Path) || string.IsNullOrEmpty(code))
            {
                ViewModel.InitViewCommand.Execute();
                return;
            }
            ViewModel.OnAuthCodeGrantedCommand.Execute(code);
        }

        private void Login(string url)
        {
            var intent = new Intent(Intent.ActionView);
            intent.SetData(AndroidUri.Parse(url));
            StartActivityForResult(intent, 0);
        }
    }
}