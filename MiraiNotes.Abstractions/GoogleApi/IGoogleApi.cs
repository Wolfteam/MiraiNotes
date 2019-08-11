using System.Threading.Tasks;
using MiraiNotes.Core.Dto.Google.Requests;
using MiraiNotes.Core.Dto.Google.Responses;
using MiraiNotes.Core.Models.GoogleApi;
using Refit;

namespace MiraiNotes.Abstractions.GoogleApi
{
    public interface IGoogleApi
    {
        #region Auth

        [Post("/oauth2/v4/token")]
        Task<TokenResponseDto> GetAccessToken([Body(BodySerializationMethod.UrlEncoded)]
            TokenRequestDto request);

        [Post("/oauth2/v4/token")]
        Task<TokenResponseDto> RenewToken([Body(BodySerializationMethod.UrlEncoded)]
            RenewTokenRequestDto request);

        #endregion

        #region User

        [Get("/oauth2/v3/userinfo")]
        [Headers("Authorization: Bearer")]
        Task<GoogleUserResponseDto> GetUserInfo();

        #endregion

        #region TaskLists

        [Get("/tasks/v1/users/@me/lists")]
        [Headers("Authorization: Bearer")]
        Task<GoogleTaskApiResponseModel<GoogleTaskListModel>> GetAllTaskLists(
            [Query] int maxResults = 100,
            [Query] string pageToken = null);

        [Get("/tasks/v1/users/@me/lists/{taskListId}")]
        [Headers("Authorization: Bearer")]
        Task<GoogleTaskListModel> GetTaskList(string taskListId);

        [Delete("/tasks/v1/users/@me/lists/{taskListId}")]
        [Headers("Authorization: Bearer")]
        Task DeleteTaskList(string taskListId);

        [Post("/tasks/v1/users/@me/lists")]
        [Headers("Authorization: Bearer")]
        Task<GoogleTaskListModel> SaveTaskList(GoogleTaskListModel taskList);

        [Put("/tasks/v1/users/@me/lists/{taskListId}")]
        [Headers("Authorization: Bearer")]
        Task<GoogleTaskListModel> UpdateTaskList(string taskListId, GoogleTaskListModel taskList);

        #endregion

        #region Tasks

        [Post("/tasks/v1/lists/{taskListId}")]
        [Headers("Authorization: Bearer")]
        Task ClearTasks(string taskListId);

        [Delete("/tasks/v1/lists/{taskListId}/tasks/{taskId}")]
        [Headers("Authorization: Bearer")]
        Task DeleteTask(string taskListId, string taskId);

        [Get("/tasks/v1/lists/{taskListId}")]
        [Headers("Authorization: Bearer")]
        Task<GoogleTaskApiResponseModel<GoogleTaskModel>> GetAllTasks(
            [Query] string taskListId,
            [Query] int maxResults = 100,
            [Query] string pageToken = null,
            [Query] bool showHidden = true);

        [Get("/tasks/v1/lists/{taskListId}/tasks/{taskId}")]
        [Headers("Authorization: Bearer")]
        Task<GoogleTaskModel> GetTask(string taskListId, string taskId);

        [Post("/tasks/v1/lists/{taskListId}/tasks")]
        [Headers("Authorization: Bearer")]
        Task<GoogleTaskModel> SaveTask(
            [Query] string taskListId,
            GoogleTaskModel task,
            [Query] string parent = null,
            [Query] string previous = null);


        [Put("/tasks/v1/lists/{taskListId}/tasks/{taskId}")]
        [Headers("Authorization: Bearer")]
        Task<GoogleTaskModel> UpdateTask(string taskListId, string taskId, GoogleTaskModel task);
        #endregion
    }
}