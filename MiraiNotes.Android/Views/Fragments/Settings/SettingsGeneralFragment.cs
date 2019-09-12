﻿using Android.OS;
using Android.Views;
using Android.Widget;
using MiraiNotes.Android.ViewModels.Settings;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.UI;
using MvvmCross.ViewModels;

namespace MiraiNotes.Android.Views.Fragments
{
    [MvxFragmentPresentation(typeof(SettingsMainViewModel), Resource.Id.SettingsContentFrame, AddToBackStack = true)]
    public class SettingsGeneralFragment : MvxFragment<SettingsGeneralViewModel>
    {
        private IMvxInteraction<MvxColor> _onAccentColorSelected;

        public IMvxInteraction<MvxColor> OnAccentColorSelected
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

        private void SetSelectedItem(MvxColor color)
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