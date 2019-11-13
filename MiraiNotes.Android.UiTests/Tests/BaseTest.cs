using MiraiNotes.Android.UiTests.Pages;
using MiraiNotes.Android.UiTests.Pages.Dialogs;
using NUnit.Framework;
using Xamarin.UITest;

namespace MiraiNotes.Android.UiTests
{
    [TestFixture(Platform.Android)]
    public class BaseTest
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
        protected TaskMenuOptionsDialog TaskMenuOptionsDialog { get; private set; }
        protected ManageTaskListsDialog ManageTaskListsDialog { get; private set; }

        protected BaseTest(Platform platform)
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
            TaskMenuOptionsDialog = new TaskMenuOptionsDialog();
            ManageTaskListsDialog = new ManageTaskListsDialog();

            LoginPage.Login();
        }

        protected void AddNewTaskList(string title)
        {
            //Arrange

            //Act
            TasksPage.OpenNewTaskListDialog();
            string hint = TasksPage.GetHint();
            TasksPage.AddNewTaskList(title);

            //Assert
            Assert.AreEqual(hint, "Title");
        }
        //TODO: I SHOULD SET A DEFAULT LANGUAGE
    }
}
