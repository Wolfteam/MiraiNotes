using System.Linq;
using Query = System.Func<Xamarin.UITest.Queries.AppQuery, Xamarin.UITest.Queries.AppQuery>;

namespace MiraiNotes.Android.UiTests.Pages
{
    public class TasksPage : BasePage
    {
        private readonly Query _mainFabButton;
        private readonly Query _addTaskListButton;
        private readonly Query _addTaskButton;
        private readonly Query _uniqueEditTextInDialog;

        private readonly Query _yesButton;
        private readonly Query _noButton;
        private readonly Query _cancelButton;
        private readonly Query _updateButton;

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
            _cancelButton = x => x.Button("Cancel");
            _updateButton = x => x.Button("Update");
        }

        public TasksPage OpenDrawer()
        {
            //App.SwipeLeftToRight(0.99, 1000, true);
            App.Tap(x => x.Marked("Open"));

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

        public void ChangeTaskStatusFromSwipedTask(bool changeIt)
        {
            if (changeIt)
                App.Tap(_yesButton);
            else
                App.Tap(_noButton);

            App.WaitForNoElement(_yesButton);
        }

        public void DeleteTaskFromSwipedTask(bool delete)
        {
            if (delete)
                App.Tap(_yesButton);
            else
                App.Tap(_noButton);

            App.WaitForNoElement(_yesButton);
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

        public TasksPage ShowSubTasks(int taskIndex = 0)
        {
            App.Tap(x => x.Id(TaskRecyclerViewId).Child().Index(taskIndex).Descendant("AppCompatImageButton"));

            return this;
        }

        public int GetNumberOfSubTasks(int taskIndex)
        {
            return App.Query(x => x.Id(TaskRecyclerViewId).Child().Index(taskIndex).Descendant("MvxRecyclerView").Child())
                .Length;
        }
    }
}
