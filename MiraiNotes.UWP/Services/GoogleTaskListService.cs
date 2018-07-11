using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models.API;
using Newtonsoft.Json;

namespace MiraiNotes.UWP.Services
{
    public class GoogleTaskListService : IGoogleTaskListService
    {
        public const string BASE_ADDRESS = "https://www.googleapis.com/tasks/v1/users/@me/lists";

        private readonly IHttpClientsFactory _httpClientsFactory;
        private readonly IUserCredentialService _userCredentialService;

        public GoogleTaskListService(IHttpClientsFactory httpClientsFactory, IUserCredentialService userCredentialService)
        {
            _httpClientsFactory = httpClientsFactory;
            _userCredentialService = userCredentialService;
        }

        public async Task<GoogleEmptyResponseModel> DeleteAsync(string taskListID)
        {
            var result = new GoogleEmptyResponseModel();
            var httpclient = _httpClientsFactory.GetHttpClient();
            var response = await httpclient.DeleteAsync($"{BASE_ADDRESS}/{taskListID}");
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                result.Errors = JsonConvert.DeserializeObject<GoogleResponseErrorModel>(responseBody);
                return result;
            }

            result.Succeed = true;
            return result;
        }

        public async Task<GoogleResponseModel<GoogleTaskApiResponseModel<GoogleTaskListModel>>> GetAllAsync(
            int maxResults = 100, 
            string pageToken = null)
        {
            var result = new GoogleResponseModel<GoogleTaskApiResponseModel<GoogleTaskListModel>>();
            var httpClient = _httpClientsFactory.GetHttpClient();
            var response = await httpClient.GetAsync(BASE_ADDRESS);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                result.Errors = JsonConvert.DeserializeObject<GoogleResponseErrorModel>(responseBody);
                result.Succeed = false;
                return result;
            }
            result.Succeed = true;
            result.Result = JsonConvert.DeserializeObject<GoogleTaskApiResponseModel<GoogleTaskListModel>>(responseBody);
            return result;
        }

        public async Task<GoogleResponseModel<GoogleTaskListModel>> GetAsync(string taskListID)
        {
            var result = new GoogleResponseModel<GoogleTaskListModel>();
            var httpClient = _httpClientsFactory.GetHttpClient();
            var response = await httpClient.GetAsync($"{BASE_ADDRESS}/{taskListID}");
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                result.Errors = JsonConvert.DeserializeObject<GoogleResponseErrorModel>(responseBody);
                return result;
            }
            result.Succeed = true;
            result.Result = JsonConvert.DeserializeObject<GoogleTaskListModel>(responseBody);
            return result;
        }

        public async Task<GoogleResponseModel<GoogleTaskListModel>> SaveAsync(GoogleTaskListModel taskList)
        {
            var result = new GoogleResponseModel<GoogleTaskListModel>();
            var httpClient = _httpClientsFactory.GetHttpClient();
            string json = JsonConvert.SerializeObject(taskList);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(BASE_ADDRESS, stringContent);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                result.Errors = JsonConvert.DeserializeObject<GoogleResponseErrorModel>(responseBody);
                return result;
            }
            result.Succeed = true;
            result.Result = JsonConvert.DeserializeObject<GoogleTaskListModel>(responseBody);
            return result;
        }

        public async Task<GoogleResponseModel<GoogleTaskListModel>> UpdateAsync(string taskListID, GoogleTaskListModel taskList)
        {
            var result = new GoogleResponseModel<GoogleTaskListModel>();
            var httpClient = _httpClientsFactory.GetHttpClient();
            string json = JsonConvert.SerializeObject(taskList);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"{BASE_ADDRESS}/{taskListID}", stringContent);

            string responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                result.Errors = JsonConvert.DeserializeObject<GoogleResponseErrorModel>(responseBody);
                return result;
            }
            result.Succeed = true;
            result.Result = JsonConvert.DeserializeObject<GoogleTaskListModel>(responseBody);
            return result;
        }
    }
}
