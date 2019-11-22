using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Dto.Google.Responses;
using MiraiNotes.Core.Models.GoogleApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Design
{
    public class DesignGoogleApiService : IGoogleApiService
    {
        public Task<EmptyResponseDto> ClearTasks(string taskListId)
        {
            throw new NotImplementedException();
        }

        public Task<EmptyResponseDto> DeleteTask(string taskListId, string taskId)
        {
            throw new NotImplementedException();
        }

        public Task<EmptyResponseDto> DeleteTaskList(string taskListId)
        {
            var result = new EmptyResponseDto
            {
                Succeed = true
            };
            return Task.FromResult(result);
        }

        public Task<ResponseDto<TokenResponseDto>> GetAccessTokenAsync(string approvalCode)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<GoogleTaskApiResponseModel<GoogleTaskListModel>>> GetAllTaskLists(int maxResults = 100, string pageToken = null)
        {
            var items = new List<GoogleTaskListModel>()
            {
                new GoogleTaskListModel
                {
                    TaskListID = "1",
                    Title = "Design-1",
                    UpdatedAt = DateTime.Now
                },
                new GoogleTaskListModel
                {
                    TaskListID = "2",
                    Title = "Design-2",
                    UpdatedAt = DateTime.Now
                },
                new GoogleTaskListModel
                {
                    TaskListID = "3",
                    Title = "Design-3",
                    UpdatedAt = DateTime.Now
                }
            };

            var results = new GoogleTaskApiResponseModel<GoogleTaskListModel>()
            {
                Items = items
            };

            var response = new ResponseDto<GoogleTaskApiResponseModel<GoogleTaskListModel>>()
            {
                Result = results,
                Succeed = true
            };
            return Task.FromResult(response);
        }

        public Task<ResponseDto<GoogleTaskApiResponseModel<GoogleTaskModel>>> GetAllTasks(string taskListId, int maxResults = 100, string pageToken = null, bool showHidden = true)
        {
            var items = new List<GoogleTaskModel>()
            {
                new GoogleTaskModel
                {
                    Notes = "Cuerpo de la nota 1",
                    Position = "1",
                    TaskID = "1",
                    Title = "Task 1"
                },
                new GoogleTaskModel
                {
                    Notes = "Cuerpo de la nota 2",
                    Position = "2",
                    TaskID = "2",
                    Title = "Task 2"
                },
                new GoogleTaskModel
                {
                    Notes = "Cuerpo de la nota 2",
                    Position = "2",
                    TaskID = "2",
                    Title = "Task 2 "
                }
            };

            var wrapper = new GoogleTaskApiResponseModel<GoogleTaskModel>()
            {
                Items = items
            };

            var response = new ResponseDto<GoogleTaskApiResponseModel<GoogleTaskModel>>()
            {
                Succeed = true,
                Result = wrapper
            };

            return Task.FromResult(response);
        }

        public string GetAuthorizationUrl()
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<TokenResponseDto>> GetNewTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<GoogleTaskModel>> GetTask(string taskListId, string taskId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<GoogleTaskListModel>> GetTaskList(string taskListId)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<GoogleUserResponseDto>> GetUser()
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<GoogleTaskModel>> SaveTask(string taskListId, GoogleTaskModel task, string parent = null, string previous = null)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<GoogleTaskListModel>> SaveTaskList(GoogleTaskListModel taskList)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<TokenResponseDto>> SignInWithGoogle()
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<GoogleTaskModel>> UpdateTask(string taskListId, string taskId, GoogleTaskModel task)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<GoogleTaskListModel>> UpdateTaskList(string taskListId, GoogleTaskListModel taskList)
        {
            throw new NotImplementedException();
        }
    }
}
