using MiraiNotes.Shared.Models;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Services
{
    public abstract class BaseGoogleAuthService : IGoogleAuthService
    {
        #region Constans

        private const string ClientId = "442000266621-ueo5g3sfnqe20sn2k0gaf51aoo45tar6.apps.googleusercontent.com";
        private const string ClientSecret = "WhPYeY662TkXKDGryBqUZP7n";

        #endregion

        #region Members

        private readonly IReadOnlyList<string> _scopes = new List<string>
        {
            "https://www.googleapis.com/auth/tasks",
            "https://www.googleapis.com/auth/userinfo.profile",
            "https://www.googleapis.com/auth/userinfo.email"
        };

        #endregion

        #region Properties

        public string ProviderName => "GOOGLE";

        public string AuthorizationEndpoint => "https://accounts.google.com/o/oauth2/auth";

        public string TokenEndpoint => "https://accounts.google.com/o/oauth2/token";

        public string RefreshTokenEndpoint => "https://www.googleapis.com/oauth2/v4/token";

        public string RedirectUrl => "urn:ietf:wg:oauth:2.0:oob:auto";

        public string ApprovalUrl => "https://accounts.google.com/o/oauth2/approval";

        #endregion

        public string GetAuthorizationUrl()
        {
            return $"{AuthorizationEndpoint}" +
                   $"?client_id={Uri.EscapeDataString(ClientId)}" +
                   $"&scope={string.Join(" ", _scopes)}" +
                   $"&redirect_uri={Uri.EscapeDataString(RedirectUrl)}" +
                   $"&response_type=code" +
                   $"&include_granted_scopes=true";
        }

        public async Task<TokenResponse> GetNewTokenAsync(string refreshToken)
        {
            string tokenRequestBody = $"refresh_token={refreshToken}" +
                                      $"&client_id={ClientId}" +
                                      $"&client_secret={ClientSecret}" +
                                      $"&grant_type=refresh_token";

            var content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = null;

            try
            {
                var response = await httpClient.PostAsync(RefreshTokenEndpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
                tokenResponse.RefreshToken = refreshToken;
                return tokenResponse;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<TokenResponse> GetTokenAsync(string approvalCode)
        {
            // Builds the Token request
            string tokenRequestBody = $"code={approvalCode}" +
                                      $"&client_id={ClientId}" +
                                      $"&redirect_uri={Uri.EscapeDataString(RedirectUrl)}" +
                                      $"&client_secret={ClientSecret}" +
                                      $"&scope=&grant_type=authorization_code";

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
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseBody);
                return tokenResponse;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public abstract Task<Response<TokenResponse>> SignInWithGoogle();
    }
}