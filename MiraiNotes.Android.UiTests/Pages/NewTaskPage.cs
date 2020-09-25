using MiraiNotes.Android.UiTests.Extensions;
using System;
using System.Linq;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace MiraiNotes.Android.UiTests.Pages
{
    public class NewTaskPage : BasePage
    {
        private readonly Query _saveChangesButton;
        private readonly Query _discardChangesButton;

        private readonly Query _reminderButton;
        private readonly Query _completitionDateButton;
        private readonly Query _moveButton;
        private readonly Query _addSubTaskButton;
        
        private readonly Query _cancelButton;
        private readonly Query _okButton;
        private readonly Query _closeButton;
        private readonly Query _tasReminderDateDialogButton;

        private readonly Query _taskTitleEditText;
        private readonly Query _taskContentEditText;

        public override PlatformQuery Trait { get; }

        public NewTaskPage()
        {
            _saveChangesButton = x => x.Id("SaveTask");
            _discardChangesButton = x => x.Id("DiscardChanges");

            _reminderButton = x => x.Button("Add / Edit a reminder");
            _completitionDateButton = x => x.Id("CompletitionDateButton");
            _moveButton = x => x.Button("Move");
            _addSubTaskButton = x => x.Button("Add a subtask");

            _cancelButton = x => x.Button("Cancel");
            _tasReminderDateDialogButton = x => x.Button("TaskReminderDate");
            _okButton = x => x.Button("Ok");
            _closeButton = x => x.Button("Close");

            _taskTitleEditText = x => x.Id("TaskTitle");
            _taskContentEditText = x => x.Id("TaskNotes");

            Trait = new PlatformQuery
            {
                Android = _discardChangesButton
            };
        }

        public void AddEditNewTask(string title, string content)
        {
            ClearTaskTitle();
            ClearTaskContent();

            SetTaskTitle(title);
            SetTaskContent(content);

            App.Tap(_saveChangesButton);

            App.WaitForNoElement(_saveChangesButton, timeout: TimeSpan.FromSeconds(5));
        }

        public NewTaskPage ShowAddSubTaskDialog()
        {
            App.Tap(_addSubTaskButton);
            App.WaitForElement(_cancelButton);

            return this;
        }

        public NewTaskPage ShowAddEditCompletitionDate()
        {
            App.Tap(_completitionDateButton);
            App.WaitForElement(_tasReminderDateDialogButton);

            return this;
        }

        public NewTaskPage AddCompletitionDate(bool addIt)
        {
            App.Tap(_tasReminderDateDialogButton);
            App.WaitForElement(x => x.Class(AppPickerExtensions.AndroidDatePickerClass));
            
            App.UpdateDatePicker(AppManager.Platform, DateTime.Now.AddDays(10));
            var okDatePickerButton = App.Query(x => x.Class("ButtonBarLayout").Child()).FirstOrDefault(b => b.Text == "OK");
            App.Tap(x => x.Id(okDatePickerButton.Id));

            //Save the changes ?
            if (addIt)
                App.Tap(_okButton);
            else
                App.Tap(_closeButton);

            App.WaitForNoElement(_okButton);

            return this;
        }

        public NewTaskPage ShowAddReminderDialog()
        {
            App.Tap(_reminderButton);
            App.WaitForElement(_okButton);

            return this;
        }

        public bool CompletitionDateIsShown()
        {
            return App.Query(x => x.Class("AppCompatTextView")).Any(x => x.Text.Contains("This task is marked"));
        }

        public void ClosePage()
        {
            App.Tap(_discardChangesButton);
            App.WaitForNoElement(_discardChangesButton);
        }

        public NewTaskPage ClearTaskTitle()
        {
            App.ClearText(_taskTitleEditText);

            return this;
        }

        public NewTaskPage SetTaskTitle(string title)
        {
            App.EnterText(_taskTitleEditText, title);
            App.DismissKeyboard();
            return this;
        }

        public NewTaskPage ClearTaskContent()
        {
            App.ClearText(_taskContentEditText);

            return this;
        }

        public NewTaskPage SetTaskContent(string content)
        {
            App.EnterText(_taskContentEditText, content);
            App.DismissKeyboard();

            return this;
        }

        public bool IsTaskTitleErrorValidationVisible()
        {
            App.Tap(_taskTitleEditText);
            return App.Query(x => x.Marked("Title cannot be empty")).Any();
        }

        public bool IsTaskContentErrorValidationVisible()
        {
            App.Tap(_taskContentEditText);
            return App.Query(x => x.Marked("Notes cannot be empty")).Any();
        }

        public NewTaskPage ShowMoveTaskDialog()
        {
            App.Tap(_moveButton);

            App.WaitForElement(x => x.Id("MoveTaskListView"));

            return this;
        }
    }
}
