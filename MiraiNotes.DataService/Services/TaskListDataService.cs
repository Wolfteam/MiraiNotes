using Microsoft.EntityFrameworkCore;
using MiraiNotes.Data;
using MiraiNotes.Data.Models;
using MiraiNotes.DataService.Interfaces;

namespace MiraiNotes.DataService.Services
{
    public class TaskListDataService : Repository<GoogleTaskList>, ITaskListDataService
    {
        public TaskListDataService(DbContext context) 
            : base(context)
        {
        }
    }
}
