using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Dto.Google.Responses;
using MiraiNotes.Core.Enums;
using MiraiNotes.Core.Models.GoogleApi;
using MiraiNotes.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiraiNotes.Shared.Services
{
    public class MockedGoogleApiService : IGoogleApiService
    {
        private readonly List<GoogleTaskListModel> _taskLists = new List<GoogleTaskListModel>
        {
            new GoogleTaskListModel
            {
                ETag = "e_tag",
                TaskListID = "1",
                Title = "The main list",
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new GoogleTaskListModel
            {
                ETag = "e_tag",
                TaskListID = "2",
                Title = "My super list",
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };
        private readonly List<GoogleTaskModel> _tasks = new List<GoogleTaskModel>
        {
            new GoogleTaskModel
            {
                CompletedOn = DateTimeOffset.UtcNow.AddDays(-10),
                Notes = "The content of the task A",
                Position = "001",
                Status = GoogleTaskStatus.COMPLETED.GetString(),
                TaskID = "1",
                Title = "Task A",
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            },
            new GoogleTaskModel
            {
                Notes = "The content of the task B",
                Position = "002",
                Status = GoogleTaskStatus.NEEDS_ACTION.GetString(),
                TaskID = "2",
                Title = "Task B",
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-5)
            },
            new GoogleTaskModel
            {
                CompletedOn = DateTimeOffset.UtcNow.AddDays(-10),
                Notes = "The content of the task C",
                Position = "003",
                Status = GoogleTaskStatus.COMPLETED.GetString(),
                TaskID = "3",
                Title = "Task C",
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-10)
            },
            new GoogleTaskModel
            {
                Notes = "The content of the task D",
                Position = "004",
                Status = GoogleTaskStatus.NEEDS_ACTION.GetString(),
                TaskID = "4",
                Title = "Task D",
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new GoogleTaskModel
            {
                Notes = "The content of the task E",
                Position = "005",
                Status = GoogleTaskStatus.NEEDS_ACTION.GetString(),
                TaskID = "5",
                Title = "Task E",
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-3)
            }
        };

        public Task<EmptyResponseDto> ClearTasks(string taskListId)
        {
            var response = new EmptyResponseDto
            {
                Succeed = true
            };

            return Task.FromResult(response);
        }

        public Task<EmptyResponseDto> DeleteTask(string taskListId, string taskId)
        {
            var response = new EmptyResponseDto
            {
                Succeed = true
            };

            return Task.FromResult(response);
        }

        public Task<EmptyResponseDto> DeleteTaskList(string taskListId)
        {
            var response = new EmptyResponseDto
            {
                Succeed = true
            };

            return Task.FromResult(response);
        }

        public Task<ResponseDto<TokenResponseDto>> GetAccessTokenAsync(string approvalCode)
        {
            var response = new ResponseDto<TokenResponseDto>()
            {
                Result = new TokenResponseDto
                {
                    AccessToken = "Access token",
                    RefreshToken = "Refresh token",
                    TokenId = "tokenid",
                    ExpiresIn = 3600,
                    TokenType = "grant_type"
                },
                Succeed = true
            };

            return Task.FromResult(response);
        }

        public Task<ResponseDto<GoogleTaskApiResponseModel<GoogleTaskListModel>>> GetAllTaskLists(int maxResults = 100, string pageToken = null)
        {
            var response = new ResponseDto<GoogleTaskApiResponseModel<GoogleTaskListModel>>
            {
                Succeed = true,
                Result = new GoogleTaskApiResponseModel<GoogleTaskListModel>
                {
                    ETag = "e_tag",
                    Items = new List<GoogleTaskListModel>
                    {
                        new GoogleTaskListModel
                        {
                            ETag = "e_tag",
                            TaskListID = "1",
                            Title = "The main list",
                            UpdatedAt = DateTimeOffset.UtcNow
                        },
                        new GoogleTaskListModel
                        {
                            ETag = "e_tag",
                            TaskListID = "2",
                            Title = "My super list",
                            UpdatedAt = DateTimeOffset.UtcNow
                        }
                    }
                }
            };

            return Task.FromResult(response);
        }

        public Task<ResponseDto<GoogleTaskApiResponseModel<GoogleTaskModel>>> GetAllTasks(string taskListId, int maxResults = 100, string pageToken = null, bool showHidden = true)
        {
            var response = new ResponseDto<GoogleTaskApiResponseModel<GoogleTaskModel>>
            {
                Succeed = true,
                Result = new GoogleTaskApiResponseModel<GoogleTaskModel>
                {
                    ETag = "e_tag"
                }
            };
            if (taskListId == "1")
            {
                response.Result.Items = _tasks.Take(2);
            }
            else
            {
                response.Result.Items = _tasks.Skip(2);
            }

            return Task.FromResult(response);
        }

        public string GetAuthorizationUrl()
        {
            return AppConstants.BaseGoogleAuthUrl;
        }

        public Task<ResponseDto<TokenResponseDto>> GetNewTokenAsync(string refreshToken)
        {
            return GetAccessTokenAsync(refreshToken);
        }

        public Task<ResponseDto<GoogleTaskModel>> GetTask(string taskListId, string taskId)
        {
            var response = new ResponseDto<GoogleTaskModel>
            {
                Succeed = true,
                Result = _tasks.First(t => t.TaskID == taskId)
            };
            return Task.FromResult(response);
        }

        public Task<ResponseDto<GoogleTaskListModel>> GetTaskList(string taskListId)
        {
            var response = new ResponseDto<GoogleTaskListModel>
            {
                Succeed = true,
                Result = _taskLists.First(t => t.TaskListID == taskListId)
            };
            return Task.FromResult(response);
        }

        public Task<ResponseDto<GoogleUserResponseDto>> GetUser()
        {
            var response = new ResponseDto<GoogleUserResponseDto>
            {
                Succeed = true,
                Result = new GoogleUserResponseDto
                {
                    Email = "miraisoft@gmail.com",
                    FullName = "Mirai Soft",
                    ID = "01",
                }
            };

            return Task.FromResult(response);
        }

        public Task<ResponseDto<GoogleTaskModel>> SaveTask(string taskListId, GoogleTaskModel task, string parent = null, string previous = null)
        {
            var response = new ResponseDto<GoogleTaskModel>
            {
                Succeed = true,
                Result = task
            };

            return Task.FromResult(response);
        }

        public Task<ResponseDto<GoogleTaskListModel>> SaveTaskList(GoogleTaskListModel taskList)
        {
            var response = new ResponseDto<GoogleTaskListModel>
            {
                Succeed = true,
                Result = taskList
            };

            return Task.FromResult(response);
        }

        public Task<ResponseDto<TokenResponseDto>> SignInWithGoogle()
        {
            throw new NotImplementedException();
        }

        public Task<ResponseDto<GoogleTaskModel>> UpdateTask(string taskListId, string taskId, GoogleTaskModel task)
        {
            var response = new ResponseDto<GoogleTaskModel>
            {
                Succeed = true,
                Result = task
            };

            return Task.FromResult(response);
        }

        public Task<ResponseDto<GoogleTaskListModel>> UpdateTaskList(string taskListId, GoogleTaskListModel taskList)
        {
            var response = new ResponseDto<GoogleTaskListModel>
            {
                Succeed = true,
                Result = taskList
            };

            return Task.FromResult(response);
        }
    }
}
