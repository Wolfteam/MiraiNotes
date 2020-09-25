using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xamarin.UITest.Queries;

namespace MiraiNotes.Android.UiTests.Pages
{
    public class SettingsPage : BasePage
    {
        private const int SettingsGeneralIndex = 0;
        private const int SettingsSyncIndex = 1;
        private const int SettingsNotificationsIndex = 2;
        private const int SettingsAboutIndex = 3;

        private readonly AppQuery _appThemeDialog;

        public override PlatformQuery Trait { get; }

        public SettingsPage()
        {
            Trait = new PlatformQuery
            {
                Android = x => x.Id("SettingsContentFrame")
            };

            _appThemeDialog = BuildBaseAppQuery().Id("select_dialog_listview");
        }

        public SettingsPage GoToGeneral()
        {
            GoTo(SettingsGeneralIndex);

            App.WaitForElement(x => x.Id("SettingsAccentColorsGridView"));
            return this;
        }

        public SettingsPage GoToSync()
        {
            GoTo(SettingsSyncIndex);
            App.WaitForElement(x => x.Text("Synchronization"));
            return this;
        }

        public SettingsPage GoToNotifications()
        {
            GoTo(SettingsNotificationsIndex);
            App.WaitForElement(x => x.Text("Notifications"));
            return this;
        }

        public SettingsPage GoToAbout()
        {
            GoTo(SettingsAboutIndex);
            App.WaitForElement(x => x.Text("About"));
            return this;
        }


        public SettingsPage OpenAppThemeDialog()
        {
            App.Tap(x => x.Class("MvxSpinner").Index(0));
            App.WaitForElement(x => _appThemeDialog);
            return this;
        }

        public void ChangeAppTheme()
        {
            var accentColor = GetAppBarBgColor();
            int themeIndex = IsDarkThemeSelected(accentColor) ? 1 : 0;
            App.Tap(x => _appThemeDialog.Child().Index(themeIndex));

            App.WaitForNoElement(x => _appThemeDialog);
        }

        public bool IsDarkThemeSelected(Color accentColor)
        {
            var query = BuildBaseAppQuery()
                .Id("select_dialog_listview")
                .Descendant("AppCompatTextView");

            var colors = GetTextColors(query);

            //the first item is always the dark theme
            //if its close the accent color, that means its selected
            return ColorsAreClose(colors.First(), accentColor);
        }

        public void SetAccentColor(int accentColorIndex)
        {
            App.Tap(x => x.Id("SettingsAccentColorsGridView").Child().Index(accentColorIndex));
            App.WaitForNoElement(x => x.Id("SettingsAccentColorsGridView"));
        }

        public List<Color> GetAllAccentColors()
        {
            var query = BuildBaseAppQuery().Id("SettingsAccentColorsGridView").Child();
            return GetBackgroundColors(query);
        }

        private void GoTo(int settingsIndex)
            => App.Tap(x => x.Class("MvxListView").Child().Index(settingsIndex));
    }
}
