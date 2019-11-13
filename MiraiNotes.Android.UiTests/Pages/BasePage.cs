using NUnit.Framework;
using System;

namespace MiraiNotes.Android.UiTests.Pages
{
    public abstract class BasePage : BaseView
    {
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
    }
}
