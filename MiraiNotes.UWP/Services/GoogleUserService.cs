﻿using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
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
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
