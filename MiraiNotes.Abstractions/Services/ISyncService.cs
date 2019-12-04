using System.Collections.Generic;
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

        Task<EmptyResponseDto> SyncTaskListAsync(int taskListId);

        Task<EmptyResponseDto> SyncTasksAsync(int taskListId);

        Task<List<EmptyResponseDto>> PerformSyncOnlyOn(int taskListId);

        Task<List<EmptyResponseDto>> PerformFullSync(bool isInBackground);
    }
}
