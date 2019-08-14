using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiraiNotes.Abstractions.GoogleApi;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Dto.Google.Requests;
using MiraiNotes.Core.Dto.Google.Responses;
using MiraiNotes.Core.Models.GoogleApi;
using Refit;

namespace MiraiNotes.Shared.Services
{
    public abstract class BaseGoogleApiService : IGoogleApiService
    {
        private readonly IGoogleApi _googleApiService;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUrl;

        private readonly IReadOnlyList<string> _scopes = new List<string>
        {
            "https://www.googleapis.com/auth/tasks",
            "https://www.googleapis.com/auth/userinfo.profile",
            "https://www.googleapis.com/auth/userinfo.email"
        };

        private const string GoogleRefreshGrantType = "refresh_token";
        private const string GoogleTokenGrantType = "authorization_code";


        public BaseGoogleApiService(
            IGoogleApi googleApiService,
            string clientId,
            string clientSecret,
            string redirectUrl)
        {
            _googleApiService = googleApiService;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUrl = redirectUrl;
        }

        public string GetAuthorizationUrl()
        {
            return $"{AppConstants.BaseGoogleAuthUrl}" +
                   $"?client_id={Uri.EscapeDataString(_clientId)}" +
                   $"&scope={string.Join(" ", _scopes)}" +
                   $"&redirect_uri={Uri.EscapeDataString(_redirectUrl)}" +
                   "&response_type=code" +
                   "&include_granted_scopes=true";
        }

        #region Auth

        public async Task<ResponseDto<TokenResponseDto>> GetAccessTokenAsync(string approvalCode)
        {
            var response = new ResponseDto<TokenResponseDto>();

            try
            {
                var tokenResponse = await _googleApiService.GetAccessToken(new TokenRequestDto
                {
                    ClientId = _clientId,
                    ClientSecret = _clientSecret,
                    RedirectUri = _redirectUrl,
                    GrantType = GoogleTokenGrantType,
                    ApprovalCode = approvalCode,
                });

                response.Succeed = true;
                response.Result = tokenResponse;
            }
            catch (Exception e)
            {
                response.Message = e.Message;
            }

            return response;
        }

        public async Task<ResponseDto<TokenResponseDto>> GetNewTokenAsync(string refreshToken)
        {
            var response = new ResponseDto<TokenResponseDto>();

            try
            {
                var tokenResponse = await _googleApiService.RenewToken(new RenewTokenRequestDto
                {
                    ClientId = _clientId,
                    ClientSecret = _clientSecret,
                    GrantType = GoogleRefreshGrantType,
                    RefreshToken = refreshToken
                });

                response.Succeed = true;
                response.Result = tokenResponse;
            }
            catch (Exception e)
            {
                response.Message = e.Message;
            }

            return response;
        }

        public abstract Task<ResponseDto<TokenResponseDto>> SignInWithGoogle();

        #endregion

        #region User

        public async Task<ResponseDto<GoogleUserResponseDto>> GetUser()
        {
            var response = new ResponseDto<GoogleUserResponseDto>();

            try
            {
                response.Result = await _googleApiService.GetUserInfo();
                response.Succeed = true;
            }
            catch (ApiException apiEx)
            {
                var error = await apiEx.GetContentAsAsync<GoogleResponseErrorModel>();
                if (error is null)
                {
                    response.Message = apiEx.Message;
                }
                else
                {
                    string msg = GetGoogleError(error);
                    response.Message = msg;
                }
            }
            catch (Exception e)
            {
                response.Message = e.Message;
            }

            return response;
        }

        #endregion

        #region TaskList

        public async Task<ResponseDto<GoogleTaskApiResponseModel<GoogleTaskListModel>>> GetAllTaskLists(
            int maxResults = 100,
            string pageToken = null)
        {
            var response = new ResponseDto<GoogleTaskApiResponseModel<GoogleTaskListModel>>();

            try
            {
                response.Result = string.IsNullOrEmpty(pageToken)
                    ? await _googleApiService.GetAllTaskLists(maxResults)
                    : await _googleApiService.GetAllTaskLists(pageToken, maxResults);

                response.Succeed = true;
            }
            catch (ApiException apiEx)
            {
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }


        public async Task<ResponseDto<GoogleTaskListModel>> GetTaskList(string taskListId)
        {
            var response = new ResponseDto<GoogleTaskListModel>();

            try
            {
                response.Result = await _googleApiService.GetTaskList(taskListId);
                response.Succeed = true;
            }
            catch (ApiException apiEx)
            {
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<EmptyResponseDto> DeleteTaskList(string taskListId)
        {
            var response = new ResponseDto<GoogleTaskListModel>();

            try
            {
                await _googleApiService.DeleteTaskList(taskListId);
                response.Succeed = true;
            }
            catch (ApiException apiEx)
            {
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDto<GoogleTaskListModel>> SaveTaskList(GoogleTaskListModel taskList)
        {
            var response = new ResponseDto<GoogleTaskListModel>();

            try
            {
                var googleResponse = await _googleApiService.SaveTaskList(taskList);
                response.Succeed = true;
                response.Result = googleResponse;
            }
            catch (ApiException apiEx)
            {
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDto<GoogleTaskListModel>> UpdateTaskList(
            string taskListId,
            GoogleTaskListModel taskList)
        {
            var response = new ResponseDto<GoogleTaskListModel>();

            try
            {
                var googleResponse = await _googleApiService.UpdateTaskList(taskListId, taskList);
                response.Succeed = true;
                response.Result = googleResponse;
            }
            catch (ApiException apiEx)
            {
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        #endregion

        #region Tasks

        public async Task<EmptyResponseDto> ClearTasks(string taskListId)
        {
            var response = new EmptyResponseDto();

            try
            {
                await _googleApiService.ClearTasks(taskListId);
                response.Succeed = true;
            }
            catch (ApiException apiEx)
            {
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<EmptyResponseDto> DeleteTask(string taskListId, string taskId)
        {
            var response = new EmptyResponseDto();

            try
            {
                await _googleApiService.DeleteTask(taskListId, taskId);
                response.Succeed = true;
            }
            catch (ApiException apiEx)
            {
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDto<GoogleTaskApiResponseModel<GoogleTaskModel>>> GetAllTasks(
            string taskListId,
            int maxResults = 100,
            string pageToken = null,
            bool showHidden = true)
        {
            var response = new ResponseDto<GoogleTaskApiResponseModel<GoogleTaskModel>>();

            try
            {
                if (!string.IsNullOrEmpty(pageToken))
                    response.Result = await _googleApiService.GetAllTasks(taskListId, pageToken, maxResults, showHidden);
                else
                    response.Result = await _googleApiService.GetAllTasks(taskListId, maxResults, showHidden);
                response.Succeed = true;
            }
            catch (ApiException apiEx)
            {
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }


        public async Task<ResponseDto<GoogleTaskModel>> GetTask(string taskListId, string taskId)
        {
            var response = new ResponseDto<GoogleTaskModel>();

            try
            {
                response.Result = await _googleApiService.GetTask(taskListId, taskId);
                response.Succeed = true;
            }
            catch (ApiException apiEx)
            {
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDto<GoogleTaskModel>> SaveTask(
            string taskListId,
            GoogleTaskModel task,
            string parent = null,
            string previous = null)
        {
            var response = new ResponseDto<GoogleTaskModel>();

            try
            {
                if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(previous))
                    response.Result = await _googleApiService.SaveTask(taskListId, task, parent, previous);
                else if (!string.IsNullOrEmpty(parent))
                    response.Result = await _googleApiService.SaveTask(taskListId, task, parent);
                else if (!string.IsNullOrEmpty(previous))
                    throw new NotImplementedException("I think that if you have a previous valid you need a parent one");
                else
                    response.Result = await _googleApiService.SaveTask(taskListId, task);

                response.Succeed = true;
            }
            catch (ApiException apiEx)
            {
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<ResponseDto<GoogleTaskModel>> UpdateTask(string taskListId, string taskId,
            GoogleTaskModel task)
        {
            var response = new ResponseDto<GoogleTaskModel>();

            try
            {
                var googleResponse = await _googleApiService.UpdateTask(taskListId, taskId, task);
                response.Succeed = true;
                response.Result = googleResponse;
            }
            catch (ApiException apiEx)
            {
                await HandleApiException(apiEx, response);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        #endregion

        #region Helpers

        private static async Task HandleApiException<T>(ApiException apiEx, T response) where T : EmptyResponseDto
        {
            try
            {
                var error = await apiEx.GetContentAsAsync<GoogleResponseErrorModel>();
                if (error is null)
                {
                    response.Message = apiEx.Message;
                }
                else
                {
                    string msg = GetGoogleError(error);
                    response.Message = msg;
                }
            }
            catch (Exception)
            {
                response.Message = apiEx.Message;
            }
        }

        private static string GetGoogleError(GoogleResponseErrorModel error)
        {
            if (error.ApiError is null)
                return error.ErrorDescription;
            string msg = $"{error.ErrorDescription}_{error.ApiError.Code}_{error.ApiError.Message}";
            return msg;
        }

        #endregion
    }
}