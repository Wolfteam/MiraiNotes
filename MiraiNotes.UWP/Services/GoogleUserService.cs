using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using MiraiNotes.UWP.Utils;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Services
{
    public class GoogleUserService : IGoogleUserService
    {
        const string USER_INFO_ENDPOINT = "https://www.googleapis.com/oauth2/v3/userinfo";
        const string USER_IMAGE_FILE_NAME = "user_image.png";

        private readonly IHttpClientsFactory _httpClientsFactory;

        public GoogleUserService(IHttpClientsFactory httpClientsFactory)
        {
            _httpClientsFactory = httpClientsFactory;
        }

        public async Task<GoogleUserModel> GetUserInfoAsync()
        {
            var httpClient = _httpClientsFactory.GetHttpClient();
            try
            {
                var response = await httpClient.GetAsync(USER_INFO_ENDPOINT);
                if (!response.IsSuccessStatusCode)
                    return null;
                string responseBody = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<GoogleUserModel>(responseBody);
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task RemoveProfileImage()
        {
            await MiscellaneousUtils.RemoveFile(USER_IMAGE_FILE_NAME);
        }

        public async Task DownloadProfileImage(string url)
        {
            try
            {
                var client = _httpClientsFactory.GetHttpClient();
                var imageBytes = await client.GetByteArrayAsync(url);
                await MiscellaneousUtils.SaveFile(USER_IMAGE_FILE_NAME, imageBytes);
            }
            catch (Exception)
            {
                //Http excep..
            }
        }

        public string GetCurrentUserProfileImagePath()
            => $"{MiscellaneousUtils.GetApplicationPath()}/{USER_IMAGE_FILE_NAME}";
    }
}
