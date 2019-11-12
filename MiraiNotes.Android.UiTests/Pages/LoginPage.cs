using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace MiraiNotes.Android.UiTests.Pages
{
    public class LoginPage : BasePage<LoginPage>
    {
        private readonly Query _loginButton;
        public override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Marked("Login")
        };

        public LoginPage()
        {
            _loginButton = x => x.Button("Login");
        }

        public override LoginPage OpenDrawer(bool open = true)
        {
            OpenDrawer();
            return this;
        }

        public void Login()
        {
            App.WaitForElement(_loginButton);
            App.Invoke("BypassSignIn");
            App.WaitForNoElement(x => x.Marked("Welcome to Mirai Notes"));
        }
    }
}
