namespace MiraiNotes.Abstractions.Data
{
    public interface IMiraiNotesDataService
    {
        IUserDataService UserService { get; }
        ITaskListDataService TaskListService { get; }
        ITaskDataService TaskService { get; }
    }
}
