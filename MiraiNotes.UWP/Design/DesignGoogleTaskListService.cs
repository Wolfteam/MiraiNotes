using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiraiNotes.Core.Models.GoogleApi;
using MiraiNotes.UWP.Interfaces;

namespace MiraiNotes.UWP.Design
{
    public class DesignGoogleTaskListService : IGoogleTaskListService
    {
        public Task<GoogleEmptyResponseModel> DeleteAsync(string taskListID)
        {
            var result = new GoogleEmptyResponseModel
            {
                Succeed = true
            };
            return Task.FromResult(result);
        }

        public Task<GoogleResponseModel<GoogleTaskApiResponseModel<GoogleTaskListModel>>> GetAllAsync(int maxResults = 100, string pageToken = null)
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

            var response =  new GoogleResponseModel<GoogleTaskApiResponseModel<GoogleTaskListModel>>()
            {
                Result = results,
                Succeed = true
            };
            return Task.FromResult(response);
        }

        public Task<GoogleResponseModel<GoogleTaskListModel>> GetAsync(string taskListID)
        {
            throw new NotImplementedException();
        }

        public Task<GoogleResponseModel<GoogleTaskListModel>> SaveAsync(GoogleTaskListModel taskList)
        {
            throw new NotImplementedException();
        }

        public Task<GoogleResponseModel<GoogleTaskListModel>> UpdateAsync(string taskListID, GoogleTaskListModel taskList)
        {
            throw new NotImplementedException();
        }
    }
}
