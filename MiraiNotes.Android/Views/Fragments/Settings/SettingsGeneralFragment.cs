using Android.Animation;
using Android.OS;
using Android.Views;
using Android.Widget;
using MiraiNotes.Android.Common.Utils;
using MiraiNotes.Android.ViewModels.Settings;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.ViewModels;
using System.Drawing;

namespace MiraiNotes.Android.Views.Fragments
{
    [MvxFragmentPresentation(typeof(SettingsMainViewModel), Resource.Id.SettingsContentFrame, AddToBackStack = true, EnterAnimation = Resource.Animator.slide_enter_right_to_left, ExitAnimation = Resource.Animator.slide_exit_right_to_left, PopEnterAnimation = Resource.Animator.slide_enter_left_to_right, PopExitAnimation = Resource.Animator.slide_exit_left_to_right)]
    public class SettingsGeneralFragment : MvxFragment<SettingsGeneralViewModel>
    {
        private IMvxInteraction<Color> _onAccentColorSelected;

        public IMvxInteraction<Color> OnAccentColorSelected
        {
            get => _onAccentColorSelected;
            set
            {
                if (_onAccentColorSelected != null)
                    _onAccentColorSelected.Requested -= (sender, args) => SetSelectedItem(args.Value);

                _onAccentColorSelected = value;
                _onAccentColorSelected.Requested += (sender, args) => SetSelectedItem(args.Value);
            }
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.SettingsGeneralView, null);

            var set = this.CreateBindingSet<SettingsGeneralFragment, SettingsGeneralViewModel>();
            set.Bind(this).For(v => v.OnAccentColorSelected).To(vm => vm.OnAccentColorSelected).OneWay();
            set.Apply();

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            var gridView = Activity.FindViewById<MvxGridView>(Resource.Id.SettingsAccentColorsGridView);
            gridView.Post(() => SetSelectedItem(ViewModel.SelectedAccentColor));
        }

        public override Animator OnCreateAnimator(int transit, bool enter, int nextAnim)
        {
            if (nextAnim == 0)
                return base.OnCreateAnimator(transit, enter, nextAnim);
            return AndroidUtils.CreateSlideAnimator(Activity, nextAnim);
        }

        private void SetSelectedItem(Color color)
        {
            var gridView = Activity.FindViewById<MvxGridView>(Resource.Id.SettingsAccentColorsGridView);
            var index = ViewModel.AccentColors.IndexOf(color);
            for (int i = 0; i < gridView.ChildCount; i++)
            {
                var view = gridView.GetChildAt(i) as ImageView;

                int icon = i == index
                    ? Resource.Drawable.ic_done_24dp
                    : 0;

                view.SetImageResource(icon);
            }
        }
    }
}