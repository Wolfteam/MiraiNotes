using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Dto.Google.Responses;
using MiraiNotes.Shared;
using Newtonsoft.Json;

namespace MiraiNotes.UWP.Services
{
    public abstract class BaseGoogleAuthService : IGoogleApiService
    {
        #region Members

        private readonly IReadOnlyList<string> _scopes = new List<string>
        {
            "https://www.googleapis.com/auth/tasks",
            "https://www.googleapis.com/auth/userinfo.profile",
            "https://www.googleapis.com/auth/userinfo.email"
        };

        #endregion

        #region Properties

        public string TokenEndpoint => "https://accounts.google.com/o/oauth2/token";

        public string RefreshTokenEndpoint => "https://www.googleapis.com/oauth2/v4/token";

        #endregion

        public string GetAuthorizationUrl()
        {
            return $"{AppConstants.BaseGoogleAuthUrl}" +
                   $"?client_id={Uri.EscapeDataString(AppConstants.ClientId)}" +
                   $"&scope={string.Join(" ", _scopes)}" +
                   $"&redirect_uri={Uri.EscapeDataString(AppConstants.RedirectUrl)}" +
                   "&response_type=code" +
                   "&include_granted_scopes=true";
        }

        public async Task<ResponseDto<TokenResponseDto>> GetNewTokenAsync(string refreshToken)
        {
            var result = new ResponseDto<TokenResponseDto>();

            var tokenRequestBody = $"refresh_token={refreshToken}" +
                                   $"&client_id={AppConstants.ClientId}" +
                                   $"&client_secret={AppConstants.ClientSecret}" +
                                   "&grant_type=refresh_token";

            var content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = null;

            try
            {
                var response = await httpClient.PostAsync(RefreshTokenEndpoint, content);
                if (!response.IsSuccessStatusCode) return null;

                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponseDto>(responseBody);
                tokenResponse.RefreshToken = refreshToken;

                result.Result = tokenResponse;
                result.Succeed = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        public async Task<ResponseDto<TokenResponseDto>> GetAccessTokenAsync(string approvalCode)
        {
            var result = new ResponseDto<TokenResponseDto>();
            // Builds the Token request
            var tokenRequestBody = $"code={approvalCode}" +
                                   $"&client_id={AppConstants.ClientId}" +
                                   $"&redirect_uri={Uri.EscapeDataString(AppConstants.RedirectUrl)}" +
                                   $"&client_secret={AppConstants.ClientSecret}" +
                                   "&scope=&grant_type=authorization_code";

            var content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Performs the authorization code exchange.
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true
            };
            var client = new HttpClient(handler);

            try
            {
                var response = await client.PostAsync(TokenEndpoint, content);
                if (!response.IsSuccessStatusCode) return null;

                var responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponseDto>(responseBody);

                result.Result = tokenResponse;
                result.Succeed = true;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        public abstract Task<ResponseDto<TokenResponseDto>> SignInWithGoogle();
    }
}