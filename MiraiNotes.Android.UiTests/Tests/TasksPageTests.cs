using NUnit.Framework;
using Xamarin.UITest;

namespace MiraiNotes.Android.UiTests.Tests
{
    public class TasksPageTests : BaseTest
    {
        public TasksPageTests(Platform platform)
            : base(platform)
        {
        }

        #region Fab
        [TestCase("Testing")]
        public void AddNewTaskList_ShouldNotContainTasks(string title)
        {
            //Arrange - Act
            AddNewTaskList(title);

            //Assert
            int tasks = TasksPage.GetCurrentNumberOfTasks();
            //since this is a new task list, it must contain 0 tasks
            Assert.True(tasks == 0);
        }
        #endregion

        #region AppBar
        [TestCase("By name asc")]
        [TestCase("By name desc")]
        [TestCase("By updated date asc")]
        [TestCase("By updated date desc")]
        public void SorTask_By(string by)
        {
            //Arrange
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTasks();

            //Act
            TasksPage.SortTasks(by);

            //Assert
            int finalNumberOfTask = TasksPage.GetCurrentNumberOfTasks();
            Assert.True(initialNumberOfTask == finalNumberOfTask);
        }
        #endregion

        #region Drawer
        [TestCase(true)]
        [TestCase(false)]
        public void Logout_UserIsLoggedOut_ReturnsToLoginPage(bool logOut)
        {
            //Arrange
            TasksPage.OpenDrawer();

            //Act
            TasksPage.ShowLogoutDialog().Logout(logOut);

            //Assert
            if (logOut)
                Assert.DoesNotThrow(() => App.WaitForElement(LoginPage.Trait.Current));
            else
                Assert.DoesNotThrow(() => App.WaitForElement(TasksPage.Trait.Current));
            Assert.True(!TasksPage.IsDrawerOpen());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ShowAccountDialog_LoggedUserIsShown(bool openFromImg)
        {
            //Arrange
            TasksPage.OpenDrawer();

            //Act
            bool accountIsShown = TasksPage.ShowAccountDialog(openFromImg).AccountIsShown();

            //Assert
            Assert.True(accountIsShown);
            Assert.True(!TasksPage.IsDrawerOpen());
        }
        #endregion

        #region Task Swipe Actions
        [TestCase(true)]
        [TestCase(false)]
        public void SwipeTask_ToTheRight_ChangeTaskStatus(bool changeStatus)
        {
            //Arrange
            int index = 0;
            bool isTaskCompleted = TasksPage.IsTextStrikeThrough(index);

            //Act
            TasksPage.SwipeTaskTo(true, true, index).ChangeTaskStatusFromSwipedTask(changeStatus);

            //Assert
            if (changeStatus)
                Assert.True(TasksPage.IsTextStrikeThrough(index) == !isTaskCompleted);
            else
                Assert.True(TasksPage.IsTextStrikeThrough(index) == isTaskCompleted);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SwipeTask_ToTheLeft_DeleteTask(bool deleteIt)
        {
            //Arrange
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTasks();

            //Act
            TasksPage.SwipeTaskTo(false).DeleteTaskFromSwipedTask(deleteIt);

            //Assert
            int finalNumberOfTask = TasksPage.GetCurrentNumberOfTasks();
            if (deleteIt)
                Assert.True(initialNumberOfTask - 1 == finalNumberOfTask);
            else
                Assert.True(initialNumberOfTask == finalNumberOfTask);
        }
        #endregion    
    }
}
