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

        #region Fab
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
        #endregion

        #region AppBar
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

        #region Tasks Menu Option
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

        [Test]
        public void DeleteTask_FreshCreatedTask_MustBeDeleted()
        {
            //Arrange
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTask();
            TasksPage.GoToNewTaskPage();
            NewTaskPage.AddNewTask("A Task", "This task will be deleted");

            //Act
            //E.g: initially you have 3 task, create another one and you will have 4, but the corresponding
            //index will be equal to the initial number of tasks
            TasksPage.ShowTaskMenuOptions(initialNumberOfTask)
                .ShowDeleteTaskDialog()
                .DeleteTask(true);

            //Assert
            int finalNumberOfTask = TasksPage.GetCurrentNumberOfTask();
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

        [Test]
        public void AddSubTask_EmptyTitle_MustShowAValidationError()
        {
            //Arrange
            TasksPage.ShowTaskMenuOptions().ShowAddSubTaskDialog();

            //Act
            bool isInvalid = TasksPage.IsSubTaskTitleValid(string.Empty);

            //Assert
            Assert.True(isInvalid);
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
        #endregion    

        #region Manage Task Lists
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
            TasksPage.ShowDeleteTaskListDialog(taskListIndex).DeleteTaskList(deleteIt);

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
            TasksPage.ShowEditTaskListDialog(taskListIndex).EditTaskList(saveChanges, newTitle);

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

        [Test]
        public void EditTaskList_EmptyTitle_MustShowAValidationError()
        {
            //Arrange
            TasksPage.OpenDrawer()
                .ShowTaskListsManageDialog()
                .ShowEditTaskListDialog();

            //Act
            bool isInvalid = TasksPage.IsTaskListTitleInvalid(string.Empty);

            //Assert
            Assert.True(isInvalid);
        }
        #endregion
    }
}
