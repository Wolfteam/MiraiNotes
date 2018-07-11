using MiraiNotes.UWP.Interfaces;

namespace MiraiNotes.UWP.Services
{
    public class GoogleApiService : IGoogleApiService
    {
        public GoogleApiService(IGoogleUserService googleUserService,
                                IGoogleTaskListService googleTaskListService,
                                IGoogleTaskService googleTaskService)
        {
            TaskListService = googleTaskListService;
            TaskService = googleTaskService;
            UserService = googleUserService;
        }

        public IGoogleUserService UserService { get; }
        public IGoogleTaskListService TaskListService { get; }
        public IGoogleTaskService TaskService { get; }        
    }
}
