using MiraiNotes.Android.ViewModels;

namespace MiraiNotes.Android.Models.Results
{
    public class TaskListsDialogViewModelResult
    {
        public bool WasMoved { get; }
        public bool WasPartiallyMoved { get; }
        public bool WasSelected { get; }
        public bool NoChangesWereMade
            => !WasMoved && !WasSelected;
        public TaskListItemViewModel TaskList { get; }

        private TaskListsDialogViewModelResult(bool wasMoved, bool wasSelected, bool wasPartiallyMoved, TaskListItemViewModel taskList)
        {
            WasMoved = wasMoved;
            WasSelected = wasSelected;
            WasPartiallyMoved = wasPartiallyMoved;
            TaskList = taskList;
        }

        public static TaskListsDialogViewModelResult Moved(TaskListItemViewModel taskList)
        {
            return new TaskListsDialogViewModelResult(true, false, false, taskList);
        }

        public static TaskListsDialogViewModelResult PartiallyMoved(TaskListItemViewModel taskList)
        {
            return new TaskListsDialogViewModelResult(false, false, true, taskList);
        }

        public static TaskListsDialogViewModelResult Selected(TaskListItemViewModel taskList)
        {
            return new TaskListsDialogViewModelResult(false, true, false, taskList);
        }

        public static TaskListsDialogViewModelResult Nothing(TaskListItemViewModel taskList)
        {
            return new TaskListsDialogViewModelResult(false, false, false, taskList);
        }
    }
}