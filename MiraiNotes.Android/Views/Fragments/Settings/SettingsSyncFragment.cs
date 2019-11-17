using Android.OS;
using Android.Views;
using MiraiNotes.Android.ViewModels.Settings;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.ViewModels;

namespace MiraiNotes.Android.Views.Fragments
{
    [MvxFragmentPresentation(typeof(SettingsMainViewModel), Resource.Id.SettingsContentFrame, AddToBackStack = true)]
    public class SettingsSyncFragment : MvxFragment<SettingsSyncViewModel>
    {

        private IMvxInteraction _closeActivityRequest;

        public IMvxInteraction CloseActivityRequest
        {
            get => _closeActivityRequest;
            set
            {
                if (_closeActivityRequest != null)
                    _closeActivityRequest.Requested -= (sender, args) => Activity.Finish();

                _closeActivityRequest = value;
                _closeActivityRequest.Requested += (sender, args) => Activity.Finish();
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(Resource.Layout.SettingsSyncView, null);

            var set = this.CreateBindingSet<SettingsSyncFragment, SettingsSyncViewModel>();
            set.Bind(this).For(v => v.CloseActivityRequest).To(vm => vm.CloseActivity);
            set.Apply();

            return view;
        }
    }
}