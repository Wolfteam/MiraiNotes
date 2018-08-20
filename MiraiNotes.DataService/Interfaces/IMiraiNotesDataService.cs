using MiraiNotes.Shared.Models;
using System;
using System.Threading.Tasks;

namespace MiraiNotes.DataService.Interfaces
{
    public interface IMiraiNotesDataService
    {
        IUserDataService UserService { get; }
        ITaskListDataService TaskListService { get; }
        ITaskDataService TaskService { get; }
    }
}
