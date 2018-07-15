using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Design
{
    public class DesignGoogleTaskService : IGoogleTaskService
    {
        public Task<GoogleEmptyResponseModel> ClearAsync(string taskListID)
        {
            throw new NotImplementedException();
        }

        public Task<GoogleEmptyResponseModel> DeleteAsync(string taskListID, string taskID)
        {
            throw new NotImplementedException();
        }

        public Task<GoogleResponseModel<GoogleTaskApiResponseModel<GoogleTaskModel>>> GetAllAsync(string taskListID, int maxResults = 100, string pageToken = null)
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

            var response = new GoogleResponseModel<GoogleTaskApiResponseModel<GoogleTaskModel>>()
            {
                Succeed = true,
                Result = wrapper
            };

            return Task.FromResult(response);
        }

        public Task<GoogleResponseModel<GoogleTaskModel>> GetAsync(string taskListID, string taskID)
        {
            throw new NotImplementedException();
        }

        public Task<GoogleResponseModel<GoogleTaskModel>> MoveAsync(string taskListID, string taskID, string parent = null, string previous = null)
        {
            throw new NotImplementedException();
        }

        public Task<GoogleResponseModel<GoogleTaskModel>> SaveAsync(string taskListID, GoogleTaskModel task, string parent = null, string previous = null)
        {
            throw new NotImplementedException();
        }

        public Task<GoogleResponseModel<GoogleTaskModel>> UpdateAsync(string taskListID, string taskID, GoogleTaskModel task)
        {
            throw new NotImplementedException();
        }
    }
}
