using MiraiNotes.Android.UiTests.Extensions;
using System;
using System.Drawing;
using System.Linq;
using Xamarin.UITest.Queries;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace MiraiNotes.Android.UiTests.Pages
{
    public class TasksPage : BasePage<TasksPage>
    {
        public const string TaskRecyclerViewId = "TaskRecyclerView";
        public const string MvxListViewClass = "MvxListView";
        public const string MaterialButtonClass = "MaterialButton";

        private readonly Query _mainFabButton;
        private readonly Query _addTaskListButton;
        private readonly Query _addTaskButton;
        private readonly Query _uniqueEditTextInDialog;

        private readonly Query _yesButton;
        private readonly Query _noButton;
        private readonly Query _addButton;
        private readonly Query _cancelButton;
        private readonly Query _okButton;
        private readonly Query _closeButton;
        private readonly Query _updateButton;

        private const int TaskMenuOption_DeleteTask = 0;
        private const int TaskMenuOption_ChangeTaskStatus = 1;
        private const int TaskMenuOption_AddSubTask = 2;
        private const int TaskMenuOption_MoveToDiffTaskList = 3;
        private const int TaskMenuOption_AddAReminder = 4;

        public override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Marked("AppFab")
        };

        public TasksPage()
        {
            _mainFabButton = x => x.Marked("AppFab");
            _addTaskListButton = x => x.Id("AddNewTaskListFab");
            _addTaskButton = x => x.Id("AddNewTaskFab");
            _uniqueEditTextInDialog = x => x.Class("TextInputEditText");

            _yesButton = x => x.Button("Yes");
            _noButton = x => x.Button("No");
            _addButton = x => x.Button("Add");
            _cancelButton = x => x.Button("Cancel");
            _okButton = x => x.Button("Ok");
            _closeButton = x => x.Button("Close");
            _updateButton = x => x.Button("Update");
        }

        public override TasksPage OpenDrawer(bool open = true)
        {
            OpenDrawer();
            return this;
        }

        public TasksPage OpenNewTaskListDialog()
        {
            //We tap the main fab button
            App.Tap(_mainFabButton);
            App.WaitForElement(_addTaskListButton);

            //We tap the new task list fab button
            App.Tap(_addTaskListButton);
            App.WaitForElement(_uniqueEditTextInDialog);

            return this;
        }

        public TasksPage AddNewTaskList(string title)
        {
            //We enter the task list title
            App.EnterText(_uniqueEditTextInDialog, title);

            //And finally we save the new task list
            App.Tap(x => x.Marked("Add"));
            App.WaitForNoElement(_uniqueEditTextInDialog);
            return this;
        }

        public void GoToNewTaskPage()
        {
            //We tap the main fab button
            App.Tap(_mainFabButton);
            App.WaitForElement(_addTaskButton);

            App.Tap(_addTaskButton);
        }

        public void SortTasks(string by)
        {
            App.Tap(x => x.Id("SortTasks"));

            App.WaitForElement(x => x.Marked(by));

            App.Tap(x => x.Marked(by));
        }

        public TasksPage ShowTaskMenuOptions(int index = 0)
        {
            App.TouchAndHold(x => x.Id(TaskRecyclerViewId).Child().Index(index).Child());

            App.WaitForElement(x => x.Button("Add a reminder"));
            return this;
        }

        public TasksPage ShowDeleteTaskDialog()
        {
            SelectTaskMenuOption(TaskMenuOption_DeleteTask);

            App.WaitForElement(_yesButton);
            return this;
        }

        public void DeleteTask(bool delete)
        {
            if (delete)
                App.Tap(_yesButton);
            else
                App.Tap(_noButton);

            App.WaitForNoElement(_yesButton);
        }

        public TasksPage ShowChangeTaskStatusDialog()
        {
            //just in case this task is in completed / incompleted state
            SelectTaskMenuOption(TaskMenuOption_ChangeTaskStatus);

            App.WaitForElement(_yesButton);

            return this;
        }

        public void ChangeTaskStatus(bool changeIt)
        {
            if (changeIt)
                App.Tap(_yesButton);
            else
                App.Tap(_noButton);

            App.WaitForNoElement(_yesButton);
        }

        public TasksPage ShowAddSubTaskDialog()
        {
            SelectTaskMenuOption(TaskMenuOption_AddSubTask);
            App.WaitForElement(_addButton);

            return this;
        }

        public void AddSubTask(bool saveIt, string subTask)
        {
            if (saveIt)
            {
                App.EnterText(x => x.Class("TextInputEditText"), subTask);
                App.Tap(_addButton);
            }
            else
            {
                App.Tap(_cancelButton);
            }

            App.WaitForNoElement(_addButton);
        }

        public bool IsSubTaskTitleValid(string title)
        {
            var query = new AppQuery(QueryPlatform.Android).Marked("Title cannot be empty");
            return IsTextInInputInvalid(title, query, _addButton, _uniqueEditTextInDialog, Color.Red);
        }

        public TasksPage ShowMoveToDiffTaskListDialog()
        {
            SelectTaskMenuOption(TaskMenuOption_MoveToDiffTaskList);
            App.WaitForElement(x => x.Class(MvxListViewClass));

            return this;
        }

        public void MoveToDiffTaskList(bool moveIt)
        {
            var index = App.Query(x => x.Class(MvxListViewClass).Child()).Length;
            App.Tap(x => x.Class(MvxListViewClass).Child().Index(index - 1));
            App.WaitForElement(_yesButton);

            if (moveIt)
                App.Tap(_yesButton);
            else
                App.Tap(_noButton);

            App.WaitForNoElement(_yesButton);
        }

        public TasksPage ShowAddReminderDialog()
        {
            SelectTaskMenuOption(TaskMenuOption_AddAReminder);
            App.WaitForElement(_okButton);

            return this;
        }

        public void AddAReminder(bool addIt)
        {
            //Datepicker
            App.Tap(x => x.Button("TaskReminderDate"));
            App.WaitForElement(x => x.Class("DatePicker"));

            App.UpdateDatePicker(AppManager.Platform, DateTime.Now.AddDays(5));
            var okButton = App.Query(x => x.Class("ButtonBarLayout").Child()).FirstOrDefault(b => b.Text == "OK");
            App.Tap(x => x.Id(okButton.Id));

            //Timepicker
            App.Tap(x => x.Button("TaskReminderTime"));
            App.WaitForElement(x => x.Class("TimePicker"));

            App.UpdateTimePicker(AppManager.Platform, DateTime.Now.AddHours(5));
            var okButton2 = App.Query(x => x.Class("ButtonBarLayout").Child()).FirstOrDefault(b => b.Text == "OK");
            App.Tap(x => x.Id(okButton2.Id));

            //Save the changes ?
            if (addIt)
                App.Tap(_okButton);
            else
                App.Tap(_closeButton);
        }

        public bool RemoveReminder(string reminderText)
        {
            var reminder = App.Query(x => x.All("AppCompatTextView")).Where(x => x.Text.Contains(reminderText)).First();
            App.TapCoordinates(reminder.Rect.CenterX, reminder.Rect.CenterY);

            return !App.Query(x => x.All("AppCompatTextView")).Where(x => x.Text.Contains(reminderText)).Any();
        }

        public TasksPage SwipeTaskTo(bool toTheRight, bool tapButton = true, int index = 0)
        {
            var rect = App.Query(x => x.Id(TaskRecyclerViewId).Child().Index(index)).First().Rect;
            if (toTheRight)
            {
                App.DragCoordinates(rect.X, rect.CenterY, rect.Width, rect.CenterY);

                if (tapButton)
                    App.TapCoordinates(rect.X, rect.CenterY);
            }
            else
            {
                App.DragCoordinates(rect.Width, rect.CenterY, 0, rect.CenterY);

                if (tapButton)
                    App.TapCoordinates(rect.Width, rect.CenterY);
            }

            return this;
        }

        public int GetCurrentNumberOfTask()
        {
            int tasks = App.Query(x => x.Id(TaskRecyclerViewId).Child()).Length;
            return tasks;
        }

        public bool IsTextStrikeThrough(int index)
        {
            var flag = App.Query(
                x => x.Id(TaskRecyclerViewId)
                    .Child()
                    .Index(index)
                    .Descendant()
                    .Class("AppCompatTextView")
                    .Invoke("getPaintFlags"))
                .First();

            var paintFlag = (long)flag;

            //16 == STRIKE_THRU_TEXT_FLAG
            return (paintFlag & 16) > 0;
        }

        public TasksPage ShowLogoutDialog()
        {
            App.Tap(x => x.Marked("Logout"));
            App.WaitForElement(_yesButton);

            return this;
        }

        public void Logout(bool logOut)
        {
            if (logOut)
                App.Tap(_yesButton);
            else
                App.Tap(_noButton);

            App.WaitForNoElement(_noButton);
        }

        public TasksPage ShowAccountDialog(bool openFromImg)
        {
            if (openFromImg)
                App.Tap(x => x.Class("CircleImageView"));
            else
                App.Tap(x => x.Marked("Accounts"));
            App.WaitForElement(_cancelButton);
            return this;
        }

        public bool AccountIsShown()
        {
            string user = App.Query(x => x.Id("AccountName")).First().Text;
            string email = App.Query(x => x.Id("AccountEmail")).First().Text;

            return !string.IsNullOrEmpty(user) &&
                !string.IsNullOrEmpty(email);
        }

        public TasksPage ShowTaskListsManageDialog()
        {
            App.Tap(x => x.Marked("Manage tasklists"));
            App.WaitForElement(x => x.Class(MvxListViewClass));
            return this;
        }

        public TasksPage ShowDeleteTaskListDialog(int index = 0)
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

        public TasksPage ShowEditTaskListDialog(int index = 0)
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

        private void SelectTaskMenuOption(int index)
        {
            App.Tap(x => x.All(MaterialButtonClass).Index(index));
        }

        private bool IsTextInInputInvalid(string text, AppQuery query, Query okButton, Query input, Color desiredColor)
        {
            App.ClearText(input);
            App.EnterText(input, text);

            //If the text is empty we wont be able to see the button
            if (App.Query(okButton).Any())
                App.Tap(okButton);

            //If the query to check the validator error doesnt return nothing, that means that is valid
            if (!App.Query(x => query).Any())
                return false;

            var warningColor = GetColor(query);
            bool areClose = ColorsAreClose(warningColor, desiredColor);

            bool updateButtonIsVisible = App.Query(_updateButton).Any();

            return !updateButtonIsVisible || !areClose;
        }
    }
}
