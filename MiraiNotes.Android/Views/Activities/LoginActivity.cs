using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.Views.Fragments.Dialogs;
using MvvmCross.Binding.BindingContext;
using MvvmCross.ViewModels;
using AndroidUri = Android.Net.Uri;

namespace MiraiNotes.Android.Views.Activities
{
    [Activity(
        Label = "@string/app_name",
        NoHistory = true,
        LaunchMode = LaunchMode.SingleTask
    )]
    [IntentFilter(
        actions: new[] { Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataSchemes = new[] { "com.miraisoft.notes" }
    )]
    public class LoginActivity : BaseActivity<LoginViewModel>
    {
        public override int LayoutId =>
            Resource.Layout.Login;

        private IMvxInteraction<string> _loginRequest;

        public IMvxInteraction<string> LoginRequest
        {
            get => _loginRequest;
            set
            {
                if (_loginRequest != null)
                    _loginRequest.Requested -= (sender, args) => Login(args.Value);

                _loginRequest = value;
                _loginRequest.Requested += (sender, args) => Login(args.Value);
            }
        }


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var set = this.CreateBindingSet<LoginActivity, LoginViewModel>();
            set.Bind(this).For(v => v.LoginRequest).To(vm => vm.LoginRequest).OneWay();
            set.Apply();

            ViewModel.TelemetryService.Init();
        }

        protected override void OnResume()
        {
            base.OnResume();

            var data = Intent.Data;
            var code = data?.GetQueryParameter("code");

            var sp = PreferenceManager.GetDefaultSharedPreferences(this);
            var addingAccount = sp.GetBoolean(AccountDialogFragment.AddAccountKey, false);

            if (addingAccount && !string.IsNullOrEmpty(code))
            {
                sp.Edit().PutString(AccountDialogFragment.AuthCodeKey, code).Commit();
            }

            if (data == null ||
                string.IsNullOrEmpty(data.Path) ||
                string.IsNullOrEmpty(code) ||
                addingAccount)
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