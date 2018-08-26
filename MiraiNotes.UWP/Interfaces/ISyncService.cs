using MiraiNotes.Shared.Models;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Interfaces
{
    public interface ISyncService
    {
        Task<EmptyResponse> SyncDownTaskListsAsync(bool isInBackground);
        Task<EmptyResponse> SyncDownTasksAsync(bool isInBackground);
        Task<EmptyResponse> SyncUpTaskListsAsync(bool isInBackground);
        Task<EmptyResponse> SyncUpTasksAsync(bool isInBackground);
    }
}
