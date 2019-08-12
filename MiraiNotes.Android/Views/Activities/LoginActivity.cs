using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using MiraiNotes.Android.Messages;
using MiraiNotes.Android.ViewModels;
using MvvmCross.Droid.Support.V7.AppCompat;
using System;
using System.Collections.Generic;
using Android.Arch.Lifecycle;
using MiraiNotes.Android.Common.Messages;
using AndroidUri = Android.Net.Uri;

namespace MiraiNotes.Android.Views.Activities
{
    [Activity(Label = "@string/app_name", NoHistory = true)]
    [IntentFilter(
        actions: new[] {Intent.ActionView},
        Categories = new[] {Intent.CategoryDefault, Intent.CategoryBrowsable},
        DataSchemes = new[] {"com.miraisoft.notes"}
    )]
    public class LoginActivity : MvxAppCompatActivity<LoginViewModel>
    {
        //private GoogleApiClient _googleApiClient;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Login);
            //var googleSignInButton = FindViewById<SignInButton>(Resource.Id.GoogleSignUp);
            //googleSignInButton.Click += (sender, args) => SignIn();

//            var loginButton = FindViewById<Button>(Resource.Id.loginButton);
//            loginButton.Click += (sender, args) => ViewModel.OnLoginRequest();

            var loginMsg = ViewModel.Messenger.Subscribe<LoginRequestMsg>(msg => Login(msg.Url));
            ViewModel.SubscriptionTokens.Add(loginMsg);
            //var options = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
            //    .RequestEmail()
            //    .RequestProfile()
            //    .RequestScopes(new Scope("https://www.googleapis.com/auth/tasks"))
            //    .Build();

            //var builder = new GoogleApiClient.Builder(this)
            //    .EnableAutoManage(this, this)
            //    .AddConnectionCallbacks(this)
            //    .AddOnConnectionFailedListener(this)
            //    .AddApi(Auth.GOOGLE_SIGN_IN_API, options);

            //_googleApiClient = builder.Build();
        }

        protected override void OnResume()
        {
            base.OnResume();

            var data = Intent.Data;
            var code = data?.GetQueryParameter("code");
            if (data == null || string.IsNullOrEmpty(data.Path) || string.IsNullOrEmpty(code))
            {
                ViewModel.OnLoginCanceled();
                return;
            }

            var msg = new AuthCodeGrantedMsg(this, code);
            ViewModel.Messenger.Publish(msg);
        }

        //private void SignIn()
        //{
        //    var intent = Auth.GoogleSignInApi.GetSignInIntent(_googleApiClient);
        //    StartActivityForResult(intent, 0);
        //}

        private void Login(string url)
        {
            var intent = new Intent(Intent.ActionView);
            intent.SetData(AndroidUri.Parse(url));
            StartActivity(intent);
        }
    }
}