using MiraiNotes.Shared.Models;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Interfaces
{
    public interface ISyncService
    {
        Task<Result> SyncDownTaskListsAsync(bool isInBackground);
        Task<Result> SyncDownTasksAsync(bool isInBackground);
        Task<Result> SyncUpTaskListsAsync(bool isInBackground);
        Task<Result> SyncUpTasksAsync(bool isInBackground);
    }
}
