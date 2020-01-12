using Android.Animation;
using Android.OS;
using Android.Views;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.ViewModels.Settings;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.ViewModels;

namespace MiraiNotes.Android.Views.Fragments
{
    [MvxFragmentPresentation(typeof(SettingsMainViewModel), Resource.Id.SettingsContentFrame, AddToBackStack = true, EnterAnimation = Resource.Animator.slide_enter_right_to_left, ExitAnimation = Resource.Animator.slide_exit_right_to_left, PopEnterAnimation = Resource.Animator.slide_enter_left_to_right, PopExitAnimation = Resource.Animator.slide_exit_left_to_right)]
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

        public override Animator OnCreateAnimator(int transit, bool enter, int nextAnim)
        {
            if (nextAnim == 0)
                return base.OnCreateAnimator(transit, enter, nextAnim);
            return AndroidUtils.CreateSlideAnimator(Activity, nextAnim);
        }
    }
}