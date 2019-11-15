using NUnit.Framework;
using Xamarin.UITest;

namespace MiraiNotes.Android.UiTests.Tests.Dialogs
{
    public class ManageTaskListsDialogTests : BaseTest
    {
        public ManageTaskListsDialogTests(Platform platform) 
            : base(platform)
        {
        }

        #region Manage Task Lists
        [TestCase(true)]
        [TestCase(false)]
        public void DeleteTaskList_TaskListIsNew_ShouldBeDeleted(bool deleteIt)
        {
            //Arrange
            string title = $"This tasklist will be deleted = {deleteIt}";
            AddNewTaskList(title);
            TasksPage.OpenDrawer();
            ManageTaskListsDialog.ShowMainDialog();

            int taskListIndex = ManageTaskListsDialog.GetTaskListIndexFromManageTaskListsDialogDialog(title);
            int initialNumberOfTaskLists = ManageTaskListsDialog.GetTaskListsCountFromManageTaskListsDialogDialog();

            //Act
            ManageTaskListsDialog.ShowDeleteTaskListDialog(taskListIndex).DeleteTaskList(deleteIt);

            //Assert
            if (deleteIt)
            {
                TasksPage.OpenDrawer();
                ManageTaskListsDialog.ShowMainDialog();
                int finalNumberOfTaskLists = ManageTaskListsDialog.GetTaskListsCountFromManageTaskListsDialogDialog();
                Assert.True(initialNumberOfTaskLists - 1 == finalNumberOfTaskLists);
            }
            else
            {
                //In this case the dialog is shown back
                int finalNumberOfTaskLists = ManageTaskListsDialog.GetTaskListsCountFromManageTaskListsDialogDialog();
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
            AddNewTaskList(title);
            TasksPage.OpenDrawer();
            ManageTaskListsDialog.ShowMainDialog();

            int taskListIndex = ManageTaskListsDialog.GetTaskListIndexFromManageTaskListsDialogDialog(title);

            //Act
            ManageTaskListsDialog.ShowEditTaskListDialog(taskListIndex).EditTaskList(saveChanges, newTitle);

            //Assert
            if (saveChanges)
            {
                TasksPage.OpenDrawer();
                ManageTaskListsDialog.ShowMainDialog();
            }
            else
            {
                //In this case the dialog is shown back
                newTitle = title;
            }
            Assert.True(ManageTaskListsDialog.TaskListExistsInManageTaskListsDialog(newTitle));
        }

        [Test]
        public void EditTaskList_EmptyTitle_MustShowAValidationError()
        {
            //Arrange
            TasksPage.OpenDrawer();
            ManageTaskListsDialog.ShowMainDialog().ShowEditTaskListDialog();

            //Act
            bool isInvalid = ManageTaskListsDialog.IsTaskListTitleInvalid(string.Empty);

            //Assert
            Assert.True(isInvalid);
        }
        #endregion

    }
}
