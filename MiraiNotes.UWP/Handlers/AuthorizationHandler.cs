using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Enums;
using MiraiNotes.UWP.Interfaces;
using IGoogleApiService = MiraiNotes.Abstractions.Services.IGoogleApiService;

namespace MiraiNotes.UWP.Handlers
{
    public class AuthorizationHandler : DelegatingHandler
    {
        private readonly ICustomDialogService _dialogService;
        private readonly IGoogleApiService _googleAuthService;
        private readonly IUserCredentialService _userCredentialService;

        public AuthorizationHandler(
            IUserCredentialService userCredentialService,
            IGoogleApiService googleAuthService,
            ICustomDialogService dialogService)
        {
            _userCredentialService = userCredentialService;
            _googleAuthService = googleAuthService;
            _dialogService = dialogService;
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Cloning the request, in case we need to send it again
            var clonedRequest = await CloneRequest(request);
            var response = await base.SendAsync(clonedRequest, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                try
                {
                    var currentLoggedUsername = _userCredentialService.GetCurrentLoggedUsername();
                    if (string.IsNullOrEmpty(currentLoggedUsername))
                    {
                        await _dialogService.ShowMessageDialogAsync(
                            "Error",
                            "Could't retrieve the current logged username");
                        return response;
                    }

                    var refreshToken = _userCredentialService.GetUserCredential(
                        ResourceType.REFRESH_TOKEN_RESOURCE,
                        currentLoggedUsername);
                    if (string.IsNullOrEmpty(refreshToken))
                    {
                        await _dialogService.ShowMessageDialogAsync(
                            "Error",
                            "Could't retrieve the refresh token");
                        return response;
                    }

                    var tokenResponse = await _googleAuthService.GetNewTokenAsync(refreshToken);
                    if (!tokenResponse.Succeed)
                    {
                        await _dialogService.ShowMessageDialogAsync(
                            "Error",
                            "Could't get a new token. Did you remove access to our app :C?");
                        return response;
                    }

                    var token = tokenResponse.Result;

                    // we're now logged in again.

                    // Clone the request
                    clonedRequest = await CloneRequest(request);

                    // Save the user to the app settings
                    _userCredentialService.UpdateUserCredential(
                        ResourceType.REFRESH_TOKEN_RESOURCE,
                        currentLoggedUsername,
                        false,
                        token.RefreshToken);
                    _userCredentialService.UpdateUserCredential(
                        ResourceType.TOKEN_RESOURCE,
                        currentLoggedUsername,
                        false,
                        token.AccessToken);
                    // Set the authentication header
                    //clonedRequest.Headers.Remove("Bearer");
                    //clonedRequest.Headers.Add("Bearer", newToken.AccessToken);
                    clonedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
                    // Resend the request
                    response = await base.SendAsync(clonedRequest, cancellationToken);
                }
                catch (InvalidOperationException)
                {
                    // user cancelled auth, so lets return the original response
                    return response;
                }

            return response;
        }

        private async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
        {
            var result = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var header in request.Headers)
                result.Headers.Add(header.Key, header.Value);

            if (request.Content != null && request.Content.Headers.ContentType != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync();
                var mediaType = request.Content.Headers.ContentType.MediaType;
                result.Content = new StringContent(requestBody, Encoding.UTF8, mediaType);
                foreach (var header in request.Content.Headers)
                    if (!header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                        result.Content.Headers.Add(header.Key, header.Value);
            }

            return result;
        }
    }
}