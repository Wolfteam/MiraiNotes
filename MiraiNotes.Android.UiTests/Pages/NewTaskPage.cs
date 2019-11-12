using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace MiraiNotes.Android.UiTests.Pages
{
    public class NewTaskPage : BasePage<NewTaskPage>
    {
        private readonly Query _saveChangesButton;
        private readonly Query _discardChangesButton;

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

            _taskTitleEditText = x => x.Id("TaskTitle");
            _taskContentEditText = x => x.Id("TaskNotes");
        }

        public override NewTaskPage OpenDrawer(bool open = true)
        {
            OpenDrawer();
            return this;
        }

        public void AddNewTask(string title, string content)
        {
            App.EnterText(_taskTitleEditText, title);
            App.EnterText(_taskContentEditText, content);

            App.Tap(_saveChangesButton);

            App.WaitForNoElement(_saveChangesButton);
        }
    }
}
