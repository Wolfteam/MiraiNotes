using MiraiNotes.Android.ViewModels;

namespace MiraiNotes.Android.Models.Results
{
    public class AddEditTaskListDialogViewModelResult
    {
        public bool WasCreated { get; }
        public bool WasUpdated { get; }
        public bool NoChangesWereMade
            => !WasCreated && !WasUpdated;
        public TaskListItemViewModel TaskList { get; }

        public AddEditTaskListDialogViewModelResult(bool wasCreated, bool wasUpdated, TaskListItemViewModel taskList)
        {
            WasCreated = wasCreated;
            WasUpdated = wasUpdated;
            TaskList = taskList;
        }

        public static AddEditTaskListDialogViewModelResult Created(TaskListItemViewModel taskList)
        {
            return new AddEditTaskListDialogViewModelResult(true, false, taskList);
        }

        public static AddEditTaskListDialogViewModelResult Updated(TaskListItemViewModel taskList)
        {
            return new AddEditTaskListDialogViewModelResult(false, true, taskList);
        }

        public static AddEditTaskListDialogViewModelResult Nothing()
        {
            return new AddEditTaskListDialogViewModelResult(false, false, null);
        }
    }
}