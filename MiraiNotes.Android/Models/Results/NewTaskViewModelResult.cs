using MiraiNotes.Android.ViewModels;

namespace MiraiNotes.Android.Models.Results
{
    public class NewTaskViewModelResult
    {
        public TaskItemViewModel Task { get; }
        public bool WasCreated { get; }
        public bool WasUpdated { get; }
        public bool WasDeleted { get; }
        public int ItemsAdded { get; }
        public bool NoChangesWereMade
            => !WasCreated && !WasUpdated && !WasDeleted;

        private NewTaskViewModelResult(TaskItemViewModel task, bool wasCreated, bool wasUpdated, bool wasDeleted, int itemsAdded)
        {
            Task = task;
            WasCreated = wasCreated;
            WasUpdated = wasUpdated;
            WasDeleted = wasDeleted;
            ItemsAdded = itemsAdded;
        }

        public static NewTaskViewModelResult Deleted(TaskItemViewModel task)
        {
            return new NewTaskViewModelResult(task, false, false, true, 0);
        }

        public static NewTaskViewModelResult Created(TaskItemViewModel task, int itemsAdded)
        {
            return new NewTaskViewModelResult(task, true, false, false, itemsAdded);
        }

        public static NewTaskViewModelResult Updated(TaskItemViewModel task, int itemsAdded)
        {
            return new NewTaskViewModelResult(task, false, true, false, itemsAdded);
        }

        public static NewTaskViewModelResult Nothing(TaskItemViewModel task)
        {
            return new NewTaskViewModelResult(task, false, false, false, 0);
        }
    }
}