using NUnit.Framework;
using Xamarin.UITest;

namespace MiraiNotes.Android.UiTests.Tests
{
    public class NewTaskPageTests : BaseTest
    {
        public NewTaskPageTests(Platform platform)
            : base(platform)
        {
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddNewTask_CreatesOrNotANewTaskList(bool createTaskList)
        {
            //Arrange
            string title = "Title goes here";
            string content = "Content goes here";
            string taskListTitle = "A new task list";
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTask();
            if (createTaskList)
                TasksPage.OpenNewTaskListDialog().AddNewTaskList(taskListTitle);
            TasksPage.GoToNewTaskPage();

            //Act
            NewTaskPage.AddNewTask(title, content);

            //Assert
            int finalNumberOfTasks = TasksPage.GetCurrentNumberOfTask();
            Assert.True(createTaskList ? finalNumberOfTasks == 1 : initialNumberOfTask + 1 >= finalNumberOfTasks);
        }

        [TestCase(1)]
        [TestCase(3)]
        public void AddNewTask_WithSubTasks_MustCreateXSubTasks(int subTasksToCreate)
        {
            //Arrange
            string title = "Title";
            string content = $"This task contains {subTasksToCreate} subtasks";
            int taskIndex = TasksPage.GetCurrentNumberOfTask();
            TasksPage.GoToNewTaskPage();

            //Act
            for (int i = 1; i <= subTasksToCreate; i++)
            {
                NewTaskPage.ShowAddSubTaskDialog();
                TaskMenuOptionsDialog.AddSubTask(true, $"This is subtask number = {i}");
            }
            NewTaskPage.AddNewTask(title, content);

            //Assert
            Assert.DoesNotThrow(() => TasksPage.ShowSubTasks(taskIndex));
            int numberOfSubTasks = TasksPage.GetNumberOfSubTasks(taskIndex);
            Assert.True(numberOfSubTasks == subTasksToCreate);
        }
    }
}
