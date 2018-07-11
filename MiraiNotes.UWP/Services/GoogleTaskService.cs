using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Services
{
    public class GoogleTaskService : IGoogleTaskService
    {
        public const string BASE_ADDRESS = "https://www.googleapis.com/tasks/v1/lists";
        //https://www.googleapis.com/tasks/v1/lists/MDAwNDE5MDcxMTUwMzkwODQyNjA6OTQ0ODQyMjIyOjA/tasks?maxResults=2
        private readonly IHttpClientsFactory _httpClientsFactory;
        private readonly IUserCredentialService _userCredentialService;

        public GoogleTaskService(IHttpClientsFactory httpClientsFactory, IUserCredentialService userCredentialService)
        {
            _httpClientsFactory = httpClientsFactory;
            _userCredentialService = userCredentialService;
        }

        public async Task<GoogleEmptyResponseModel> ClearAsync(string taskListID)
        {
            var result = new GoogleEmptyResponseModel();
            var httpclient = _httpClientsFactory.GetHttpClient();
            var stringContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            var response = await httpclient.PostAsync($"{BASE_ADDRESS}/{taskListID}/clear", stringContent);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                result.Errors = JsonConvert.DeserializeObject<GoogleResponseErrorModel>(responseBody);
                return result;
            }

            result.Succeed = true;
            return result;
        }

        public async Task<GoogleEmptyResponseModel> DeleteAsync(string taskListID, string taskID)
        {
            var result = new GoogleEmptyResponseModel();
            var httpclient = _httpClientsFactory.GetHttpClient();
            var response = await httpclient.DeleteAsync($"{BASE_ADDRESS}/{taskListID}/tasks/{taskID}");

            string responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                result.Errors = JsonConvert.DeserializeObject<GoogleResponseErrorModel>(responseBody);
                return result;
            }

            result.Succeed = true;
            return result;
        }

        public async Task<GoogleResponseModel<GoogleTaskApiResponseModel<GoogleTaskModel>>> GetAllAsync(
            string taskListID,
            int maxResults = 100,
            string pageToken = null)
        {
            string url = $"{BASE_ADDRESS}/{taskListID}/tasks";
            var result = new GoogleResponseModel<GoogleTaskApiResponseModel<GoogleTaskModel>>();
            var httpClient = _httpClientsFactory.GetHttpClient();
            var response = await httpClient.GetAsync(url);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                result.Errors = JsonConvert.DeserializeObject<GoogleResponseErrorModel>(responseBody);
                result.Succeed = false;
                return result;
            }
            result.Succeed = true;
            result.Result = JsonConvert.DeserializeObject<GoogleTaskApiResponseModel<GoogleTaskModel>>(responseBody);
            return result;
        }

        public async Task<GoogleResponseModel<GoogleTaskModel>> GetAsync(string taskListID, string taskID)
        {
            var result = new GoogleResponseModel<GoogleTaskModel>();
            var httpClient = _httpClientsFactory.GetHttpClient();
            var response = await httpClient.GetAsync($"{BASE_ADDRESS}/{taskListID}/tasks/{taskID}");

            string responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                result.Errors = JsonConvert.DeserializeObject<GoogleResponseErrorModel>(responseBody);
                return result;
            }
            result.Succeed = true;
            result.Result = JsonConvert.DeserializeObject<GoogleTaskModel>(responseBody);
            return result;
        }

        public Task<GoogleResponseModel<GoogleTaskModel>> MoveAsync(string taskListID, string taskID, string parent = null, string previous = null)
        {
            throw new NotImplementedException();
        }

        public async Task<GoogleResponseModel<GoogleTaskModel>> SaveAsync(string taskListID, GoogleTaskModel task, string parent = null, string previous = null)
        {
            var result = new GoogleResponseModel<GoogleTaskModel>();
            var httpClient = _httpClientsFactory.GetHttpClient();
            string json = JsonConvert.SerializeObject(task);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{BASE_ADDRESS}/{taskListID}/tasks", stringContent);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                result.Errors = JsonConvert.DeserializeObject<GoogleResponseErrorModel>(responseBody);
                return result;
            }
            result.Succeed = true;
            result.Result = JsonConvert.DeserializeObject<GoogleTaskModel>(responseBody);
            return result;
        }

        public async Task<GoogleResponseModel<GoogleTaskModel>> UpdateAsync(string taskListID, string taskID, GoogleTaskModel task)
        {
            var result = new GoogleResponseModel<GoogleTaskModel>();
            var httpClient = _httpClientsFactory.GetHttpClient();
            string json = JsonConvert.SerializeObject(task);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PutAsync($"{BASE_ADDRESS}/{taskListID}/tasks/{taskID}", stringContent);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                result.Errors = JsonConvert.DeserializeObject<GoogleResponseErrorModel>(responseBody);
                return result;
            }
            result.Succeed = true;
            result.Result = JsonConvert.DeserializeObject<GoogleTaskModel>(responseBody);
            return result;
        }
    }
}
