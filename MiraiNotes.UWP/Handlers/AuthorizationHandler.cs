using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Handlers
{
    public class AuthorizationHandler : DelegatingHandler
    {
        private readonly IUserCredentialService _userCredentialService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly ICustomDialogService _dialogService;

        public AuthorizationHandler(IUserCredentialService userCredentialService,
                                    IGoogleAuthService googleAuthService,
                                    ICustomDialogService dialogService)
        {
            _userCredentialService = userCredentialService;
            _googleAuthService = googleAuthService;
            _dialogService = dialogService;
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Cloning the request, in case we need to send it again
            var clonedRequest = await CloneRequest(request);
            var response = await base.SendAsync(clonedRequest, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Oh noes, user is not logged in - we got a 401
                //that means its time to use our refresh token
                try
                {
                    string currentLoggedUsername = _userCredentialService.GetCurrentLoggedUsername();
                    if (string.IsNullOrEmpty(currentLoggedUsername))
                    {
                        await _dialogService.ShowMessageDialogAsync(
                            "Error", 
                            "Could't retrieve the current logged username");
                        return response;
                    }

                    string refreshToken = _userCredentialService.GetUserCredential(
                        PasswordVaultResourceType.REFRESH_TOKEN_RESOURCE,
                        currentLoggedUsername);
                    if (string.IsNullOrEmpty(refreshToken))
                    {
                        await _dialogService.ShowMessageDialogAsync(
                            "Error",
                            "Could't retrieve the refresh token");
                        return response;
                    }

                    var token = await _googleAuthService.GetNewTokenAsync(refreshToken);
                    if (token is null)
                    {
                        await _dialogService.ShowMessageDialogAsync(
                            "Error", 
                            "Could't get a new token. Did you remove access to our app :C?");
                        return response;
                    }

                    // we're now logged in again.

                    // Clone the request
                    clonedRequest = await CloneRequest(request);

                    // Save the user to the app settings
                    _userCredentialService.UpdateUserCredential(
                        PasswordVaultResourceType.REFRESH_TOKEN_RESOURCE, 
                        currentLoggedUsername,
                        false,
                        token.RefreshToken);
                    _userCredentialService.UpdateUserCredential(
                        PasswordVaultResourceType.TOKEN_RESOURCE,
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
            }

            return response;
        }

        private async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
        {
            var result = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var header in request.Headers)
            {
                result.Headers.Add(header.Key, header.Value);
            }

            if (request.Content != null && request.Content.Headers.ContentType != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync();
                var mediaType = request.Content.Headers.ContentType.MediaType;
                result.Content = new StringContent(requestBody, Encoding.UTF8, mediaType);
                foreach (var header in request.Content.Headers)
                {
                    if (!header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        result.Content.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            return result;
        }
    }
}
