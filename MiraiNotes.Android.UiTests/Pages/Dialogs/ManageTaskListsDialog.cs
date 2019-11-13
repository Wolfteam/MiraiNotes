using System.Drawing;
using System.Linq;
using Xamarin.UITest.Queries;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace MiraiNotes.Android.UiTests.Pages.Dialogs
{
    public class ManageTaskListsDialog : BaseDialog
    {
        private readonly Query _cancelButton;
        private readonly Query _okButton;
        private readonly Query _updateButton;

        private readonly Query _uniqueEditTextInDialog;

        public ManageTaskListsDialog()
        {
            _uniqueEditTextInDialog = x => x.Class("TextInputEditText");

            _cancelButton = x => x.Button("Cancel");
            _okButton = x => x.Button("Ok");
            _updateButton = x => x.Button("Update");
        }

        public ManageTaskListsDialog ShowMainDialog()
        {
            App.Tap(x => x.Marked("Manage tasklists"));
            App.WaitForElement(x => x.Class(MvxListViewClass));
            return this;
        }

        public ManageTaskListsDialog ShowDeleteTaskListDialog(int index = 0)
        {
            int deleteButtonIndex = 1;
            App.Tap(x => x.Class(MvxListViewClass).Child().Index(index).Descendant(MaterialButtonClass).Index(deleteButtonIndex));
            App.WaitForElement(_cancelButton);

            return this;
        }

        public void DeleteTaskList(bool deleteIt)
        {
            if (deleteIt)
                App.Tap(_okButton);
            else
                App.Tap(_cancelButton);

            App.WaitForNoElement(_okButton);
        }

        public int GetTaskListIndexFromManageTaskListsDialogDialog(string title)
        {
            return GetAllTaskListFromManageTaskListsDialogDialog()
                .Select((e, i) => new
                {
                    Index = i,
                    e.Text
                }).First(t => t.Text == title)
                .Index;
        }

        public int GetTaskListsCountFromManageTaskListsDialogDialog()
        {
            return GetAllTaskListFromManageTaskListsDialogDialog().Length;
        }

        public string GetTaskListTextFromManageTaskListsDialog(int index = 0)
        {
            return GetAllTaskListFromManageTaskListsDialogDialog()[index].Text;
        }

        public ManageTaskListsDialog ShowEditTaskListDialog(int index = 0)
        {
            int editButtonIndex = 0;
            App.Tap(x => x.Class(MvxListViewClass).Child().Index(index).Descendant(MaterialButtonClass).Index(editButtonIndex));
            App.WaitForElement(_cancelButton);

            return this;
        }

        public void EditTaskList(bool saveChanges, string newTitle)
        {
            App.ClearText(_uniqueEditTextInDialog);
            App.EnterText(_uniqueEditTextInDialog, newTitle);

            if (saveChanges)
            {
                App.Tap(_updateButton);
            }
            else
            {
                App.Tap(_cancelButton);
            }

            App.WaitForNoElement(_cancelButton);
        }

        public bool TaskListExistsInManageTaskListsDialog(string title)
        {
            return GetAllTaskListFromManageTaskListsDialogDialog().Any(t => t.Text == title);
        }

        public bool IsTaskListTitleInvalid(string title)
        {
            var query = new AppQuery(QueryPlatform.Android).Marked("Title cannot be empty");
            return IsTextInInputInvalid(title, query, _updateButton, _uniqueEditTextInDialog, Color.Red);
        }

        private AppResult[] GetAllTaskListFromManageTaskListsDialogDialog()
        {
            return App.Query(x => x.Class(MvxListViewClass).Child().Descendant("AppCompatTextView"));
        }
    }
}
