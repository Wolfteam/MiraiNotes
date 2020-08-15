using Android.Content;
using Android.OS;
using Android.Views;
using AndroidX.Preference;
using MiraiNotes.Android.ViewModels;
using MiraiNotes.Android.ViewModels.Dialogs;
using MvvmCross.Base;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.ViewModels;
using AndroidUri = Android.Net.Uri;

namespace MiraiNotes.Android.Views.Fragments.Dialogs
{
    [MvxDialogFragmentPresentation(ActivityHostViewModelType = typeof(MainViewModel), Tag = nameof(AccountDialogFragment), Cancelable = false)]
    public class AccountDialogFragment : BaseDialogFragment<AccountDialogViewModel>
    {
        public const string AddAccountKey = "AddingAccount";
        public const string AuthCodeKey = "AuthCode";

        private IMvxInteraction<string> _onAddAccountRequest;

        public IMvxInteraction<string> OnAddAccountRequest
        {
            get => _onAddAccountRequest;
            set
            {
                if (_onAddAccountRequest != null)
                    _onAddAccountRequest.Requested -= AddAccount;

                _onAddAccountRequest = value;
                _onAddAccountRequest.Requested += AddAccount;
            }
        }

        public override int LayoutId
            => Resource.Layout.AccountsDialog;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);
            var set = this.CreateBindingSet<AccountDialogFragment, AccountDialogViewModel>();
            set.Bind(this).For(v => v.OnAddAccountRequest).To(vm => vm.OnAddAccountRequest).OneWay();
            set.Apply();

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            var sp = PreferenceManager.GetDefaultSharedPreferences(Activity);
            var addingAccount = sp.GetBoolean(AddAccountKey, false);
            var authCode = sp.GetString(AuthCodeKey, null);

            if (addingAccount && !string.IsNullOrEmpty(authCode))
            {
                ViewModel.OnAuthCodeGrantedCommand.Execute(authCode);
            }

            sp.Edit()
                .PutBoolean(AddAccountKey, false)
                .PutString(AuthCodeKey, null)
                .Commit();
        }

        private void AddAccount(object sender, MvxValueEventArgs<string> eventArgs)
        {
            var sp = PreferenceManager.GetDefaultSharedPreferences(Activity);
            sp.Edit().PutBoolean(AddAccountKey, true).Commit();

            var intent = new Intent(Intent.ActionView);
            intent.SetData(AndroidUri.Parse(eventArgs.Value));
            StartActivityForResult(intent, 100);
        }
    }
}