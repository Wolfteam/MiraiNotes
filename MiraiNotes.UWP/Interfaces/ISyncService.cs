using MiraiNotes.Shared.Models;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Interfaces
{
    public interface ISyncService
    {
        Task<EmptyResponseDto> SyncDownTaskListsAsync(bool isInBackground);
        Task<EmptyResponseDto> SyncDownTasksAsync(bool isInBackground);
        Task<EmptyResponseDto> SyncUpTaskListsAsync(bool isInBackground);
        Task<EmptyResponseDto> SyncUpTasksAsync(bool isInBackground);
    }
}
