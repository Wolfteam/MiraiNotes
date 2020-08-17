using Android.Animation;
using Android.OS;
using Android.Views;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.ViewModels.Settings;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.Platforms.Android.Views.Fragments;

namespace MiraiNotes.Android.Views.Fragments.Settings
{
    [MvxFragmentPresentation(typeof(SettingsMainViewModel), Resource.Id.SettingsContentFrame)]
    public class SettingsHomeFragment : MvxFragment<SettingsHomeViewModel>
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(Resource.Layout.SettingsHomeView, null);
            return view;
        }

        public override Animator OnCreateAnimator(int transit, bool enter, int nextAnim)
        {
            return nextAnim == 0 ? base.OnCreateAnimator(transit, enter, nextAnim) : AndroidUtils.CreateSlideAnimator(Activity, nextAnim);
        }
    }
}