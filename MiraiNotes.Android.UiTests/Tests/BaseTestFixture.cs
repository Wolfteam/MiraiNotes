using MiraiNotes.Android.UiTests.Pages;
using NUnit.Framework;
using Xamarin.UITest;

namespace MiraiNotes.Android.UiTests
{
    [TestFixture(Platform.Android)]
    public class BaseTestFixture
    {
        protected IApp App
            => AppManager.App;
        protected bool OnAndroid
            => AppManager.Platform == Platform.Android;
        protected bool OniOS
            => AppManager.Platform == Platform.iOS;

        protected LoginPage LoginPage { get; private set; }
        protected TasksPage TasksPage { get; private set; }
        protected NewTaskPage NewTaskPage { get; private set; }

        protected BaseTestFixture(Platform platform)
        {
            AppManager.Platform = platform;
        }

        [SetUp]
        public virtual void BeforeEachTest()
        {
            AppManager.StartApp();

            LoginPage = new LoginPage();
            TasksPage = new TasksPage();
            NewTaskPage = new NewTaskPage();

            Login();
        }

        public void Login()
        {
            //Arrange
            App.WaitForElement(x => x.Marked("Login"));

            //Act
            LoginPage.Login();
        }

        //TODO: I SHOULD SET A DEFAULT LANGUAGE
    }
}
