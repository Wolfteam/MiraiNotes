using MiraiNotes.Abstractions.Data;
using MiraiNotes.Abstractions.Services;
using Serilog;

namespace MiraiNotes.Shared.Services.Data
{
    public class MiraiNotesDataService : IMiraiNotesDataService
    {
        public IUserDataService UserService { get; }
        public ITaskListDataService TaskListService { get; }
        public ITaskDataService TaskService { get; }


        public MiraiNotesDataService(
            IUserDataService userDataService,
            ITaskListDataService taskListDataService,
            ITaskDataService taskDataService,
            IAppSettingsService appSettings,
            ILogger logger)
        {
            MiraiNotesContext.Init(appSettings, logger.ForContext<MiraiNotesDataService>());
            UserService = userDataService;
            TaskListService = taskListDataService;
            TaskService = taskDataService;
        }
    }
}