using NUnit.Framework;
using System;
using System.Drawing;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace MiraiNotes.Android.UiTests.Pages
{
    public abstract class BasePage<TPage>
    {
        protected IApp App => AppManager.App;

        protected bool OnAndroid => AppManager.Platform == Platform.Android;

        protected bool OniOS => AppManager.Platform == Platform.iOS;

        public abstract PlatformQuery Trait { get; }

        protected BasePage()
        {
            //AssertOnPage(TimeSpan.FromSeconds(30));
            App.Screenshot("On " + this.GetType().Name);
        }

        /// <summary>
        /// Verifies that the trait is still present. Defaults to no wait.
        /// </summary>
        /// <param name="timeout">Time to wait before the assertion fails</param>
        protected void AssertOnPage(TimeSpan? timeout = default)
        {
            var message = "Unable to verify on page: " + this.GetType().Name;

            if (timeout == null)
            {
                Assert.IsNotEmpty(App.Query(Trait.Current), message);
            }
            else
            {
                Assert.DoesNotThrow(() => App.WaitForElement(Trait.Current, timeout: timeout), message);
            }
        }

        /// <summary>
        /// Verifies that the trait is no longer present. Defaults to a 5 second wait.
        /// </summary>
        /// <param name="timeout">Time to wait before the assertion fails</param>
        protected void WaitForPageToLeave(TimeSpan? timeout = default)
        {
            timeout = timeout ?? TimeSpan.FromSeconds(5);
            var message = "Unable to verify *not* on page: " + this.GetType().Name;

            Assert.DoesNotThrow(() => App.WaitForNoElement(Trait.Current, timeout: timeout), message);
        }

        public string GetHint(string fromClass = "TextInputLayout")
        {
            return App.Query(x => x.Class(fromClass).Invoke("getHint"))
                .FirstOrDefault()
                .ToString();
        }

        public bool IsDrawerOpen()
        {
            return App.Query(x => x.Class("NavigationView")).Any();
        }

        protected void OpenDrawer()
        {
            //App.SwipeLeftToRight(0.99, 1000, true);
            App.Tap(x => x.Marked("Open"));
        }

        public void PressBackButton()
        {
            App.Back();
        }

        public Color GetColor(AppQuery baseQuery, int index = 0)
        {
            int colorValue = Convert.ToInt32(App.Query(x => baseQuery.Invoke("getCurrentTextColor"))[index]);

            var color = Color.FromArgb(colorValue);
            return color;
        }

        public bool ColorsAreClose(Color a, Color b, int threshold = 500)
        {

            var rDist = Math.Abs(a.R - b.R);
            var gDist = Math.Abs(a.G - b.G);
            var bDist = Math.Abs(a.B - b.B);

            if (rDist + gDist + bDist > threshold)
                return false;

            return true;
        }

        public abstract TPage OpenDrawer(bool open = true);
    }
}
