using MiraiNotes.Android.UiTests.Extensions;
using System;
using System.Drawing;
using System.Linq;
using Xamarin.UITest.Queries;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace MiraiNotes.Android.UiTests.Pages.Dialogs
{
    public class TaskMenuOptionsDialog : BaseDialog
    {
        private const int TaskMenuOption_DeleteTask = 0;
        private const int TaskMenuOption_ChangeTaskStatus = 1;
        private const int TaskMenuOption_AddSubTask = 2;
        private const int TaskMenuOption_MoveToDiffTaskList = 3;
        private const int TaskMenuOption_AddAReminder = 4;

        private readonly Query _yesButton;
        private readonly Query _noButton;
        private readonly Query _addButton;
        private readonly Query _cancelButton;
        private readonly Query _okButton;
        private readonly Query _closeButton;

        private readonly Query _uniqueEditTextInDialog;

        public TaskMenuOptionsDialog()
        {
            _uniqueEditTextInDialog = x => x.Class("TextInputEditText");

            _yesButton = x => x.Button("Yes");
            _noButton = x => x.Button("No");
            _addButton = x => x.Button("Add");
            _cancelButton = x => x.Button("Cancel");
            _okButton = x => x.Button("Ok");
            _closeButton = x => x.Button("Close");
        }

        public TaskMenuOptionsDialog ShowMainDialog(int index = 0)
        {
            App.TouchAndHold(x => x.Id(TaskRecyclerViewId).Child().Index(index).Child());

            App.WaitForElement(x => x.Button("Add a reminder"));
            return this;
        }

        public TaskMenuOptionsDialog ShowDeleteTaskDialog()
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

        public TaskMenuOptionsDialog ShowChangeTaskStatusDialog()
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

        public TaskMenuOptionsDialog ShowAddSubTaskDialog()
        {
            SelectTaskMenuOption(TaskMenuOption_AddSubTask);
            App.WaitForElement(_addButton);

            return this;
        }

        public void AddSubTask(bool saveIt, string subTask)
        {
            if (saveIt)
            {
                App.EnterText(_uniqueEditTextInDialog, subTask);
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

        public TaskMenuOptionsDialog ShowMoveToDiffTaskListDialog()
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

        public TaskMenuOptionsDialog ShowAddReminderDialog()
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

        private void SelectTaskMenuOption(int index)
        {
            App.Tap(x => x.All(MaterialButtonClass).Index(index));
        }
    }
}
