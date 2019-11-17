using System.Threading.Tasks;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Dto.Google.Responses;
using MiraiNotes.Core.Models.GoogleApi;

namespace MiraiNotes.Abstractions.Services
{
    public interface IGoogleApiService
    {
        string GetAuthorizationUrl();

        Task<ResponseDto<TokenResponseDto>> GetAccessTokenAsync(string approvalCode);

        Task<ResponseDto<TokenResponseDto>> GetNewTokenAsync(string refreshToken);

        Task<ResponseDto<TokenResponseDto>> SignInWithGoogle();

#if Android
        Task<ResponseDto<GoogleUserResponseDto>> GetUser();

        Task<ResponseDto<GoogleTaskApiResponseModel<GoogleTaskListModel>>> GetAllTaskLists(
            int maxResults = 100,
            string pageToken = null);

        Task<ResponseDto<GoogleTaskListModel>> GetTaskList(string taskListId);

        Task<EmptyResponseDto> DeleteTaskList(string taskListId);

        Task<ResponseDto<GoogleTaskListModel>> SaveTaskList(GoogleTaskListModel taskList);

        Task<ResponseDto<GoogleTaskListModel>> UpdateTaskList(
            string taskListId,
            GoogleTaskListModel taskList);

        Task<EmptyResponseDto> ClearTasks(string taskListId);

        Task<EmptyResponseDto> DeleteTask(string taskListId, string taskId);

        Task<ResponseDto<GoogleTaskApiResponseModel<GoogleTaskModel>>> GetAllTasks(
            string taskListId,
            int maxResults = 100,
            string pageToken = null,
            bool showHidden = true);

        Task<ResponseDto<GoogleTaskModel>> GetTask(string taskListId, string taskId);

        Task<ResponseDto<GoogleTaskModel>> SaveTask(
            string taskListId,
            GoogleTaskModel task,
            string parent = null,
            string previous = null);

        Task<ResponseDto<GoogleTaskModel>> UpdateTask(string taskListId, string taskId, GoogleTaskModel task);
#endif
    }
}