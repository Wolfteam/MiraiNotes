using System;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace MiraiNotes.Android.UiTests.Pages
{
    public class NewTaskPage : BasePage
    {
        private readonly Query _saveChangesButton;
        private readonly Query _discardChangesButton;
        private readonly Query _addSubTaskButton;
        private readonly Query _completitionDateButton;
        private readonly Query _cancelButton;

        private readonly Query _taskTitleEditText;
        private readonly Query _taskContentEditText;

        public override PlatformQuery Trait => new PlatformQuery
        {
            Android = x => x.Id("DiscardChanges")
        };

        public NewTaskPage()
        {
            _saveChangesButton = x => x.Id("SaveTask");
            _discardChangesButton = x => x.Id("DiscardChanges");
            _completitionDateButton = x => x.Id("CompletitionDateButton");
            _addSubTaskButton = x => x.Button("Add a subtask");
            _cancelButton = x => x.Button("Cancel");

            _taskTitleEditText = x => x.Id("TaskTitle");
            _taskContentEditText = x => x.Id("TaskNotes");
        }

        public void AddNewTask(string title, string content)
        {
            App.EnterText(_taskTitleEditText, title);
            App.EnterText(_taskContentEditText, content);

            App.Tap(_saveChangesButton);

            App.WaitForNoElement(_saveChangesButton);
        }

        public NewTaskPage ShowAddSubTaskDialog()
        {
            App.Tap(_addSubTaskButton);
            App.WaitForElement(_cancelButton);

            return this;
        }
    }
}
