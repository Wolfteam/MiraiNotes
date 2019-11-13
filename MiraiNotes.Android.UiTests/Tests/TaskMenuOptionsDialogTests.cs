using NUnit.Framework;
using Xamarin.UITest;

namespace MiraiNotes.Android.UiTests.Tests
{
    public class TaskMenuOptionsDialogTests : BaseTest
    {
        public TaskMenuOptionsDialogTests(Platform platform) 
            : base(platform)
        {
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DeleteTask_TaskAlreadyExists_ShouldDeleteIt(bool delete)
        {
            //Arrange
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTask();

            //Act
            TaskMenuOptionsDialog.ShowMainDialog()
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
            TaskMenuOptionsDialog.ShowMainDialog(initialNumberOfTask)
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
            TaskMenuOptionsDialog.ShowMainDialog(index)
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
            TaskMenuOptionsDialog.ShowMainDialog().ShowAddSubTaskDialog();
            string hint = TasksPage.GetHint();

            //Act
            TaskMenuOptionsDialog.AddSubTask(saveIt, "A Subtask goes here");

            //Assert
            Assert.AreEqual(hint, "Title");
        }

        [Test]
        public void AddSubTask_EmptyTitle_MustShowAValidationError()
        {
            //Arrange
            TaskMenuOptionsDialog.ShowMainDialog().ShowAddSubTaskDialog();

            //Act
            bool isInvalid = TaskMenuOptionsDialog.IsSubTaskTitleValid(string.Empty);

            //Assert
            Assert.True(isInvalid);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MoveToDiffTaskList(bool moveIt)
        {
            //Arrange
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTask();
            TaskMenuOptionsDialog.ShowMainDialog().ShowMoveToDiffTaskListDialog();

            //Act
            TaskMenuOptionsDialog.MoveToDiffTaskList(moveIt);

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
            TaskMenuOptionsDialog.ShowMainDialog().ShowAddReminderDialog();

            //Act
            TaskMenuOptionsDialog.AddAReminder(addIt);

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
            bool wasRemoved = TaskMenuOptionsDialog.ShowMainDialog()
                .ShowAddReminderDialog()
                .RemoveReminder(reminderContainsText);

            //Assert
            Assert.True(wasRemoved);
        }
    }
}
