using NUnit.Framework;
using Xamarin.UITest;

namespace MiraiNotes.Android.UiTests.Tests
{
    public class LoginPageTests : BaseTestFixture
    {
        public LoginPageTests(Platform platform)
            : base(platform)
        {
        }

        [Test]
        public void Login_MockedUser_NavigatesToTaskPage()
        {
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Logout_ShouldLogout(bool logout)
        {
            LoginPage.OpenDrawer();

        }
    }
}
