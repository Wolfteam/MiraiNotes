using NUnit.Framework;
using Xamarin.UITest;

namespace MiraiNotes.Android.UiTests.Tests
{
    public class LoginPageTests : BaseTest
    {
        public LoginPageTests(Platform platform)
            : base(platform)
        {
        }

        [Test]
        public void Login_MockedUser_NavigatesToTaskPage()
        {
        }
    }
}
