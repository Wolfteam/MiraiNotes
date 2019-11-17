using System.Threading.Tasks;
using MiraiNotes.Core.Dto;

namespace MiraiNotes.Abstractions.Services
{
    public interface ISyncService
    {
        Task<EmptyResponseDto> SyncDownTaskListsAsync(bool isInBackground);
        Task<EmptyResponseDto> SyncDownTasksAsync(bool isInBackground);
        Task<EmptyResponseDto> SyncUpTaskListsAsync(bool isInBackground);
        Task<EmptyResponseDto> SyncUpTasksAsync(bool isInBackground);
    }
}
