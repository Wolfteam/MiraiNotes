using MiraiNotes.Data;
using MiraiNotes.DataService.Interfaces;

namespace MiraiNotes.DataService.Services
{
    public class MiraiNotesDataService : IMiraiNotesDataService
    {
        public IUserDataService UserService { get; }
        public ITaskListDataService TaskListService { get; }
        public ITaskDataService TaskService { get; }


        public MiraiNotesDataService(
            IUserDataService userDataService,
            ITaskListDataService taskListDataService,
            ITaskDataService taskDataService)
        {
            MiraiNotesContext.Init();
            UserService = userDataService;
            TaskListService = taskListDataService;
            TaskService = taskDataService;
        }
    }
}
