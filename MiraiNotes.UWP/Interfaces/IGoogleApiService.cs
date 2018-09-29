namespace MiraiNotes.UWP.Interfaces
{
    public interface IGoogleApiService
    {
        IGoogleUserService UserService { get; }
        IGoogleTaskListService TaskListService { get; }
        IGoogleTaskService TaskService { get; }        
    }
}
