using NUnit.Framework;
using System;
using System.Linq;
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
            int initialNumberOfTask = createTaskList
                ? 0 :
                TasksPage.GetCurrentNumberOfTasks();
            int taskIndex = initialNumberOfTask;
            if (createTaskList)
                TasksPage.OpenNewTaskListDialog().AddNewTaskList(taskListTitle);
            TasksPage.GoToNewTaskPage();

            //Act
            NewTaskPage.AddEditNewTask(title, content);

            //Assert
            int finalNumberOfTasks = TasksPage.GetCurrentNumberOfTasks();
            Assert.True(createTaskList ? finalNumberOfTasks == 1 : initialNumberOfTask + 1 >= finalNumberOfTasks);
            Assert.AreEqual(title, TasksPage.GetTaskTitle(taskIndex));
            Assert.AreEqual(content, TasksPage.GetTaskContent(taskIndex));
        }

        [TestCase(1)]
        [TestCase(3)]
        public void AddNewTask_WithSubTasks_MustCreateXSubTasks(int subTasksToCreate)
        {
            //Arrange
            string title = "Title";
            string content = $"This task contains {subTasksToCreate} subtasks";
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTasks();
            int taskIndex = initialNumberOfTask;
            TasksPage.GoToNewTaskPage();

            //Act
            for (int i = 1; i <= subTasksToCreate; i++)
            {
                NewTaskPage.ShowAddSubTaskDialog();
                TaskMenuOptionsDialog.AddSubTask(true, $"This is subtask number = {i}");
            }
            NewTaskPage.AddEditNewTask(title, content);

            //Assert
            int finalNumberOfTasks = TasksPage.GetCurrentNumberOfTasks();
            Assert.True(initialNumberOfTask + 1 == finalNumberOfTasks);
            Assert.DoesNotThrow(() => TasksPage.ShowSubTasks(taskIndex));
            int numberOfSubTasks = TasksPage.GetNumberOfSubTasks(taskIndex);
            Assert.True(numberOfSubTasks == subTasksToCreate);
            Assert.AreEqual(title, TasksPage.GetTaskTitle(taskIndex));
            Assert.AreEqual(content, TasksPage.GetTaskContent(taskIndex));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddNewTask_WithACompletitionDate_ShouldShowACompletitionDateIndicator(bool addCompletitionDate)
        {
            //Arrange
            string title = "Title";
            string content = $"This task has a completition date = {addCompletitionDate}";
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTasks();
            int taskIndex = initialNumberOfTask;
            TasksPage.GoToNewTaskPage();

            //Act
            NewTaskPage.ShowAddEditCompletitionDate().AddCompletitionDate(addCompletitionDate);
            bool dateWasAdded = NewTaskPage.CompletitionDateIsShown();
            NewTaskPage.AddEditNewTask(title, content);

            //Assert
            int finalNumberOfTasks = TasksPage.GetCurrentNumberOfTasks();
            Assert.True(initialNumberOfTask + 1 == finalNumberOfTasks);
            Assert.AreEqual(title, TasksPage.GetTaskTitle(taskIndex));
            Assert.AreEqual(content, TasksPage.GetTaskContent(taskIndex));
            if (addCompletitionDate)
            {
                Assert.True(dateWasAdded);
                Assert.True(TasksPage.HasACompletitionDateSet(taskIndex));
            }
            else
            {
                Assert.True(!dateWasAdded);
                Assert.True(!TasksPage.HasACompletitionDateSet(taskIndex));
            }
        }

        [Test]
        public void AddNewTask_UserCancelsCreationOfTask_ShouldGoToTheTasksPage()
        {
            //Arrange
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTasks();

            //Act
            TasksPage.GoToNewTaskPage();
            NewTaskPage.ClosePage();
            int finalNumberOfTasks = TasksPage.GetCurrentNumberOfTasks();

            //Assert
            Assert.AreEqual(initialNumberOfTask, finalNumberOfTasks);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddEditTask_InvalidTitleAndContent_ShowsAValidationError(bool addTask)
        {
            //Arrange
            string title = string.Empty;
            string content = string.Empty;
            if (addTask)
                TasksPage.GoToNewTaskPage();
            else
                TasksPage.GoToNewTaskPage(TasksPage.GetCurrentNumberOfTasks() - 1);

            //Act - Assert
            Assert.Throws<Exception>(() => NewTaskPage.AddEditNewTask(title, content));
            Assert.True(NewTaskPage.IsTaskTitleErrorValidationVisible());
            Assert.True(NewTaskPage.IsTaskContentErrorValidationVisibile());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EditTask_CreatesOrNotANewTaskList(bool createTaskList)
        {
            //Arrange
            string newTitle = "Edited title";
            string newContent = "This task is edited";
            AddNewTask_CreatesOrNotANewTaskList(createTaskList);
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTasks();
            int taskIndex = initialNumberOfTask - 1;

            //Act
            TasksPage.GoToNewTaskPage(taskIndex);
            NewTaskPage.AddEditNewTask(newTitle, newContent);

            //Assert
            int finalNumberOfTasks = TasksPage.GetCurrentNumberOfTasks();
            Assert.True(initialNumberOfTask == finalNumberOfTasks);
            Assert.AreEqual(newTitle, TasksPage.GetTaskTitle(taskIndex));
            Assert.AreEqual(newContent, TasksPage.GetTaskContent(taskIndex));
        }

        [Test]
        public void EditTask_ExistingTask_TaskIsTotallyEdited()
        {
            //Arrange
            string newTitle = "Edited title";
            string newContent = "This task is edited";
            string subTaskTitle = "This is subtask of an edited task";
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTasks();
            int taskIndex = initialNumberOfTask - 1;
            TasksPage.GoToNewTaskPage(taskIndex);

            //Act
            //Here we basically edit everything
            NewTaskPage.ShowAddEditCompletitionDate()
                .AddCompletitionDate(true)
                .ShowAddReminderDialog();
            TaskMenuOptionsDialog.AddAReminder(true);
            NewTaskPage.ShowAddSubTaskDialog();
            TaskMenuOptionsDialog.AddSubTask(true, subTaskTitle);
            NewTaskPage.AddEditNewTask(newTitle, newContent);

            //Assert
            int finalNumberOfTasks = TasksPage.GetCurrentNumberOfTasks();
            Assert.AreEqual(initialNumberOfTask, finalNumberOfTasks);
            Assert.AreEqual(newTitle, TasksPage.GetTaskTitle(taskIndex));
            Assert.AreEqual(newContent, TasksPage.GetTaskContent(taskIndex));
            Assert.True(TasksPage.HasACompletitionDateSet(taskIndex));
            Assert.True(TasksPage.HasAReminderDateSet(taskIndex));
            TasksPage.ShowSubTasks(taskIndex);
            Assert.True(TasksPage.HasSubTasks(taskIndex));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MoveTask_ShouldBeMoved(bool moveIt)
        {
            //Arrange
            int initialNumberOfTask = TasksPage.GetCurrentNumberOfTasks();
            int taskIndex = initialNumberOfTask - 1;
            TasksPage.GoToNewTaskPage(taskIndex);
            NewTaskPage.ShowMoveTaskDialog();
            int tasksListIndex = TaskMenuOptionsDialog.GetSelectedTaskListIndex();
            int maxIndex = TaskMenuOptionsDialog.GetNumberOfTaskLists();
            int selectedTaskListIndex = Enumerable.Range(0, maxIndex).First(value => value != tasksListIndex);

            //Act
            TaskMenuOptionsDialog.MoveToDiffTaskList(moveIt, selectedTaskListIndex);

            //Assert
            if (moveIt)
            {
                NewTaskPage.WaitForPageToLeave();
                TasksPage.AssertOnPage();
                int finalNumberOfTasks = TasksPage.GetCurrentNumberOfTasks();
                Assert.AreEqual(initialNumberOfTask - 1, finalNumberOfTasks);
            }
            else
            {
                ManageTaskListsDialog.AssertOnPage(TimeSpan.FromSeconds(10));
                Assert.True(tasksListIndex == TaskMenuOptionsDialog.GetSelectedTaskListIndex());
            }
        }
    }
}
