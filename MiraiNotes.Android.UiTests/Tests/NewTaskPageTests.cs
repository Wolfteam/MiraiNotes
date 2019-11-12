using NUnit.Framework;
using Xamarin.UITest;

namespace MiraiNotes.Android.UiTests.Tests
{
    public class NewTaskPageTests : BaseTestFixture
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
            int initialNumberOfTask = App.Query(x => x.Class("MvxRecyclerView").Child()).Length;

            //Act
            if (createTaskList)
            {
                TasksPage.OpenNewTaskListDialog()
                    .AddNewTaskList(taskListTitle);
            }
            TasksPage.GoToNewTaskPage();
            NewTaskPage.AddNewTask(title, content);

            //Assert
            int finalNumberOfTasks = App.Query(x => x.Class("MvxRecyclerView").Child()).Length;
            Assert.True(createTaskList ? finalNumberOfTasks == 1 : initialNumberOfTask + 1 >= finalNumberOfTasks);
        }
    }
}
