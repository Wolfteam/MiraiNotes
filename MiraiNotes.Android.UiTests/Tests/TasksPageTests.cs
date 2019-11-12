using NUnit.Framework;
using Xamarin.UITest;

namespace MiraiNotes.Android.UiTests.Tests
{
    public class TasksPageTests : BaseTestFixture
    {
        public TasksPageTests(Platform platform)
            : base(platform)
        {
        }

        [TestCase("Testing")]
        public void AddNewTaskList_ShouldNotContainTasks(string title)
        {
            //Arrange

            //Act
            TasksPage.OpenNewTaskListDialog();
            string hint = TasksPage.GetHint();
            TasksPage.AddNewTaskList(title);

            //Assert
            int tasks = TasksPage.GetCurrentNumberOfTask();
            //since this is a new task list, it must contain 0 tasks
            Assert.True(tasks == 0);

            Assert.AreEqual(hint, "Title");
        }

        [TestCase("By name asc")]
        [TestCase("By name desc")]
        [TestCase("By updated date asc")]
        [TestCase("By updated date desc")]
        public void SorTask_By(string by)
        {
            //Arrange
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTask();

            //Act
            TasksPage.SortTasks(by);

            //Assert
            int finalNumberOfTask = TasksPage.GetCurrentNumberOfTask();
            Assert.True(initialNumberOfTask == finalNumberOfTask);
        }

        //TODO: DELETE A FRESH CREATED TASK
        [TestCase(true)]
        [TestCase(false)]
        public void DeleteTask_TaskAlreadyExists_ShouldDeleteIt(bool delete)
        {
            //Arrange
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTask();

            //Act
            TasksPage.ShowTaskMenuOptions()
                .ShowDeleteTaskDialog()
                .DeleteTask(delete);

            //Assert
            int finalNumberOfTask = TasksPage.GetCurrentNumberOfTask();
            if (delete)
                Assert.True(initialNumberOfTask - 1 == finalNumberOfTask);
            else
                Assert.True(initialNumberOfTask == finalNumberOfTask);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ChangeTaskStatus_TaskAlreadyExists_ShouldChangeIt(bool changeIt)
        {
            //Arrange
            int index = 0;
            bool isTaskCompleted = TasksPage.IsTextStrikeThrough(index);

            //Act
            TasksPage.ShowTaskMenuOptions(index)
                .ShowChangeTaskStatusDialog()
                .ChangeTaskStatus(changeIt);

            //Assert
            if (changeIt)
                Assert.True(TasksPage.IsTextStrikeThrough(index) == !isTaskCompleted);
            else
                Assert.True(TasksPage.IsTextStrikeThrough(index) == isTaskCompleted);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddSubTask_ToExistingTask_ShouldSaveIt(bool saveIt)
        {
            //Arrange
            TasksPage.ShowTaskMenuOptions().ShowAddSubTaskDialog();
            string hint = TasksPage.GetHint();

            //Act
            TasksPage.AddSubTask(saveIt, "A Subtask goes here");

            //Assert
            Assert.AreEqual(hint, "Title");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MoveToDiffTaskList(bool moveIt)
        {
            //Arrange
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTask();
            TasksPage.ShowTaskMenuOptions().ShowMoveToDiffTaskListDialog();

            //Act
            TasksPage.MoveToDiffTaskList(moveIt);

            //Assert
            int finalNumberOfTask = TasksPage.GetCurrentNumberOfTask();
            if (moveIt)
                Assert.True(initialNumberOfTask - 1 == finalNumberOfTask);
            else
                Assert.True(initialNumberOfTask == finalNumberOfTask);
        }

        //TODO: HOW SHOULD I VALIDATE ADD REMINDER ?
        [TestCase(true)]
        [TestCase(false)]
        public void AddAReminder(bool addIt)
        {
            //Arrange
            TasksPage.ShowTaskMenuOptions().ShowAddReminderDialog();

            //Act
            TasksPage.AddAReminder(addIt);

            //Assert
            if (addIt)
            {
                Assert.DoesNotThrow(() => App.WaitForElement(x => x.Marked("Reminder was successfully created")));
            }
        }

        [Test]
        public void DeleteReminder()
        {
            //Arrange
            string reminderContainsText = "A reminder date was set";
            AddAReminder(true);

            //Act
            bool wasRemoved = TasksPage.ShowTaskMenuOptions()
                .ShowAddReminderDialog()
                .RemoveReminder(reminderContainsText);

            //Assert
            Assert.True(wasRemoved);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SwipeTask_ToTheRight_ChangeTaskStatus(bool changeStatus)
        {
            //Arrange
            int index = 0;
            bool isTaskCompleted = TasksPage.IsTextStrikeThrough(index);

            //Act
            TasksPage.SwipeTaskTo(true, true, index).ChangeTaskStatus(changeStatus);

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
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTask();

            //Act
            TasksPage.SwipeTaskTo(false).DeleteTask(deleteIt);

            //Assert
            int finalNumberOfTask = TasksPage.GetCurrentNumberOfTask();
            if (deleteIt)
                Assert.True(initialNumberOfTask - 1 == finalNumberOfTask);
            else
                Assert.True(initialNumberOfTask == finalNumberOfTask);
        }

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

        [Test]
        public void ShowAccountDialog_LoggedUserIsShown()
        {
            //Arrange
            TasksPage.OpenDrawer();

            //Act
            bool accountIsShown = TasksPage.ShowAccountDialog().AccountIsShown();

            //Assert
            Assert.True(accountIsShown);
            Assert.True(!TasksPage.IsDrawerOpen());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DeleteTaskList_TaskListIsNew_ShouldBeDeleted(bool deleteIt)
        {
            //Arrange
            string title = $"This tasklist will be deleted = {deleteIt}";
            AddNewTaskList_ShouldNotContainTasks(title);
            TasksPage.OpenDrawer().ShowTaskListsManageDialog();

            int taskListIndex = TasksPage.GetTaskListIndexFromManageTaskListsDialogDialog(title);
            int initialNumberOfTaskLists = TasksPage.GetTaskListsCountFromManageTaskListsDialogDialog();

            //Act
            TasksPage.DeleteTaskList(deleteIt, taskListIndex);

            //Assert
            if (deleteIt)
            {
                TasksPage.OpenDrawer().ShowTaskListsManageDialog();
                int finalNumberOfTaskLists = TasksPage.GetTaskListsCountFromManageTaskListsDialogDialog();
                Assert.True(initialNumberOfTaskLists - 1 == finalNumberOfTaskLists);
            }
            else
            {
                //In this case the dialog is shown back
                int finalNumberOfTaskLists = TasksPage.GetTaskListsCountFromManageTaskListsDialogDialog();
                Assert.True(initialNumberOfTaskLists == finalNumberOfTaskLists);
            }
        }


        [TestCase(true)]
        [TestCase(false)]
        public void EditTaskList_TaskListIsNew_ShouldBeEdited(bool saveChanges)
        {
            //Arrange
            string title = $"This tasklist will be edited = {saveChanges}";
            string newTitle = "The new tasklist title";
            AddNewTaskList_ShouldNotContainTasks(title);
            TasksPage.OpenDrawer().ShowTaskListsManageDialog();

            int taskListIndex = TasksPage.GetTaskListIndexFromManageTaskListsDialogDialog(title);

            //Act
            TasksPage.EditTaskList(saveChanges, newTitle, taskListIndex);

            //Assert
            if (saveChanges)
            {
                TasksPage.OpenDrawer().ShowTaskListsManageDialog();
            }
            else
            {
                //In this case the dialog is shown back
                newTitle = title;
            }
            Assert.True(TasksPage.TaskListExistsInManageTaskListsDialog(newTitle));
        }
        //TODO: I SHOULD VALIDATE WHEN AN INPUT IS SHOWN AND THE TEXT IS EMPTY THAT A RED MSG APPEARS
    }
}
