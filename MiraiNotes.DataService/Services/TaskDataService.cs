using Microsoft.EntityFrameworkCore;
using MiraiNotes.Data;
using MiraiNotes.Data.Models;
using MiraiNotes.DataService.Interfaces;

namespace MiraiNotes.DataService.Services
{
    public class TaskDataService : Repository<GoogleTask>, ITaskDataService
    {
        public TaskDataService(DbContext context)
            : base(context)
        {
        }
    }
}
