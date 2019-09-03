using Android.App;
using Android.Content.PM;
using Android.OS;
using MiraiNotes.Android.ViewModels.Settings;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Platforms.Android.Presenters.Attributes;

namespace MiraiNotes.Android.Views.Activities
{
    [MvxActivityPresentation]
    [Activity(
        Label = "Settings", 
        LaunchMode = LaunchMode.SingleTask)]
    public class SettingsActivity : MvxAppCompatActivity<SettingsMainViewModel>
    {
        //private IMvxInteraction<SettingsPageType> _onSettingItemSelected;

        //public IMvxInteraction<SettingsPageType> OnSettingItemSelected
        //{
        //    get => _onSettingItemSelected;
        //    set
        //    {
        //        if (_onSettingItemSelected != null)
        //            _onSettingItemSelected.Requested -= (sender, args) => SetFragment(args.Value);

        //        _onSettingItemSelected = value;
        //        _onSettingItemSelected.Requested += (sender, args) => SetFragment(args.Value);
        //    }
        //}


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Settings);

            if (savedInstanceState == null)
                ViewModel.InitViewCommand.Execute();

            SupportActionBar.Title = "Settings";
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
        }

        //public override bool OnOptionsItemSelected(IMenuItem item)
        //{
        //    if (SupportFragmentManager.BackStackEntryCount > 0)
        //    {
        //        SupportFragmentManager.PopBackStack();
        //        return true;
        //    }
        //    OnBackPressed();
        //    return base.OnOptionsItemSelected(item);
        //}

        public override bool OnSupportNavigateUp()
        {
            OnBackPressed();
            return true;
        }

        //private void SetFragment(SettingsPageType page)
        //{
        //    MvxFragment fragment;
        //    string title;
        //    switch (page)
        //    {
        //        case SettingsPageType.GENERAL:
        //            title = ViewModel["General"];
        //            fragment = new SettingsGeneralFragment();
        //            break;
        //        case SettingsPageType.SYNCHRONIZATION:
        //            title = ViewModel["Synchronization"];
        //            fragment = new SettingsSyncFragment();
        //            break;
        //        case SettingsPageType.NOTIFICATIONS:
        //            title = ViewModel["Notifications"];
        //            fragment = new SettingsNotificationsFragment();
        //            break;
        //        case SettingsPageType.ABOUT:
        //            title = ViewModel["About"];
        //            fragment = new SettingsAboutFragment();
        //            break;
        //        case SettingsPageType.HOME:
        //            title = ViewModel["Settings"];
        //            fragment = new SettingsHomeFragment();
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(page), page, "Invalid settings page");
        //    }

        //    SupportActionBar.Title = title;
        //    SupportActionBar.SetDisplayHomeAsUpEnabled(true);

        //    fragment.ViewModel = ViewModel;
        //    var trans = SupportFragmentManager.BeginTransaction();
        //    //trans.SetCustomAnimations(Resource.Animator.animation_slide_btm,
        //    //    Resource.Animator.animation_fade_out,
        //    //    Resource.Animator.animation_slide_btm,
        //    //    Resource.Animator.animation_fade_out);
        //    trans.Replace(Resource.Id.SettingsContentFrame, fragment);
        //    if (page != SettingsPageType.HOME)
        //        trans.AddToBackStack(null);

        //    trans.CommitAllowingStateLoss();
        //}
    }
}