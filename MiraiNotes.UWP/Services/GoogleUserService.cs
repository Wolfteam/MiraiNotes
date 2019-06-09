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

        public string GetUserProfileImagePath(string googleUserId)
            => MiscellaneousUtils.GetUserProfileImagePath(googleUserId);

        public async Task RemoveProfileImage(string googleUserId)
        {
            string filename = MiscellaneousUtils.BuildImageFilename(googleUserId);
            await MiscellaneousUtils.RemoveFile(filename);
        }

        public async Task DownloadProfileImage(string url, string googleUserId)
        {
            string filename = MiscellaneousUtils.BuildImageFilename(googleUserId);
            try
            {
                var client = _httpClientsFactory.GetHttpClient();
                var imageBytes = await client.GetByteArrayAsync(url);
                await MiscellaneousUtils.SaveFile(filename, imageBytes);
            }
            catch (Exception)
            {
                //Http excep..
            }
        }
    }
}
