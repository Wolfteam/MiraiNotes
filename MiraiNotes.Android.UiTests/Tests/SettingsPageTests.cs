using NUnit.Framework;
using System;
using System.Linq;
using Xamarin.UITest;

namespace MiraiNotes.Android.UiTests.Tests
{
    public class SettingsPageTests : BaseTest
    {
        public SettingsPageTests(Platform platform) 
            : base(platform)
        {
        }

        [Test]
        public void ChangeTheme_ThemeIsChanged()
        {
            //Arrange
            TasksPage.OpenDrawer();
            var currentThemeColor = TasksPage.GetNavigationViewBgColor();
            TasksPage.GoToSettings();

            //Act
            SettingsPage.GoToGeneral()
                .OpenAppThemeDialog()
                .ChangeAppTheme();

            //Assert
            //Since we are changing the theme, the activity must be restarted
            TasksPage.AssertOnPage(TimeSpan.FromSeconds(10));
            var finalThemeColor = TasksPage.GetNavigationViewBgColor();
            Assert.True(currentThemeColor != finalThemeColor);
        }

        [Test]
        public void ChangeAccentColor_AccentColorIsChanged()
        {
            //Arrange
            var currentAccentColor = TasksPage.GetAppBarBgColor();
            TasksPage.OpenDrawer().GoToSettings();
            var colors = SettingsPage.GoToGeneral().GetAllAccentColors();
            var accentColor = colors.First(c => !SettingsPage.ColorsAreClose(c, currentAccentColor));
            int accentColorIndex = colors.IndexOf(accentColor);

            //Act
            SettingsPage.SetAccentColor(accentColorIndex);

            //Assert
            //Since we are changing the theme, the activity must be restarted
            TasksPage.AssertOnPage(TimeSpan.FromSeconds(10));
            var finalAccentColor = TasksPage.GetAppBarBgColor();
            Assert.True(TasksPage.ColorsAreClose(finalAccentColor, accentColor));
        }

        [Test]
        public void NavigateThroughSettings_EndsInTheTaskPage()
        {
            //Arrange
            TasksPage.OpenDrawer().GoToSettings();

            //Act
            SettingsPage.GoToGeneral();
            SettingsPage.PressBackButton();
            SettingsPage.AssertOnPage();

            SettingsPage.GoToNotifications();
            SettingsPage.PressBackButton();
            SettingsPage.AssertOnPage();

            SettingsPage.GoToSync();
            SettingsPage.PressBackButton();
            SettingsPage.AssertOnPage();

            SettingsPage.GoToAbout();
            SettingsPage.PressBackButton();
            SettingsPage.AssertOnPage();

            SettingsPage.PressBackButton();

            //Assert
            Assert.DoesNotThrow(() => TasksPage.AssertOnPage());
        }
    }
}
