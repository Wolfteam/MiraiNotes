using Android.OS;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using MiraiNotes.Android.ViewModels.Settings;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;

namespace MiraiNotes.Android.Views.Fragments
{
    [MvxFragmentPresentation(typeof(SettingsMainViewModel), Resource.Id.SettingsContentFrame, AddToBackStack = true)]
    public class SettingsAboutFragment : MvxFragment<SettingsAboutViewModel>
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            var view = this.BindingInflate(Resource.Layout.SettingsAboutView, null);

            var githubLink = view.FindViewById<TextView>(Resource.Id.GitHubLink);

            githubLink.MovementMethod = LinkMovementMethod.Instance;
            return view;
        }
    }
}