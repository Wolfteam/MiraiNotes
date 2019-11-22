using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Enums;
using Serilog;

namespace MiraiNotes.Shared.Helpers
{
    public class AuthenticatedHttpClientHandler : HttpClientHandler
    {
        private readonly ILogger _logger;
        private readonly Func<IGoogleApiService> _getGoogleAuthService;
        private readonly Func<IUserCredentialService> _getUserCredentialService;

        public AuthenticatedHttpClientHandler(
            ILogger logger,
            Func<IGoogleApiService> getGoogleAuthService,
            Func<IUserCredentialService> getUserCredentialService)
        {
            _logger = logger;
            _getGoogleAuthService = getGoogleAuthService;
            _getUserCredentialService = getUserCredentialService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // See if the request has an authorize header
            var auth = request.Headers.Authorization;
            if (auth is null)
            {
                _logger.Warning(
                    $"{nameof(SendAsync)}: This request doesnt have a auth header, it will be sent as it is");
                return await base.SendAsync(request, cancellationToken);
            }

            var userCredentialService = _getUserCredentialService();

            //in this case notice that im not checking for nulls..
            var currentLoggedUsername = userCredentialService.GetCurrentLoggedUsername();
            if (string.IsNullOrEmpty(currentLoggedUsername))
            {
                _logger.Error(
                    $"{nameof(SendAsync)}: Couldnt find the current logged user");
                throw new NullReferenceException(
                    $"Username cannot be null while trying to authenticate using {auth.Scheme}");
            }

            var token = userCredentialService.GetUserCredential(
                ResourceType.TOKEN_RESOURCE,
                currentLoggedUsername);

            if (string.IsNullOrEmpty(token))
            {
                _logger.Error(
                    $"{nameof(SendAsync)}: Couldnt find a token for user = {currentLoggedUsername}");
                throw new NullReferenceException(
                    $"A token should be set before trying to to authenticate using {auth.Scheme}");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, token);

            // Cloning the request, in case we need to send it again
            var clonedRequest = await CloneRequest(request);
            var response = await base.SendAsync(clonedRequest, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            try
            {
                _logger.Information(
                    $"{nameof(SendAsync)}: Response indicates unathorized, trying to " +
                    $"get a new access token for user = {currentLoggedUsername}");

                var refreshToken = userCredentialService.GetUserCredential(
                    ResourceType.REFRESH_TOKEN_RESOURCE,
                    currentLoggedUsername);
                if (string.IsNullOrEmpty(refreshToken))
                {
                    _logger.Error(
                       $"{nameof(SendAsync)}: Couldnt find a refresh token for user = {currentLoggedUsername}");
                    throw new NullReferenceException(
                        $"A refresh token should be available before trying to to authenticate using {auth.Scheme}");
                }

                var googleAuthService = _getGoogleAuthService();
                var tokenResponse = await googleAuthService.GetNewTokenAsync(refreshToken);
                if (!tokenResponse.Succeed)
                {
                    _logger.Error(
                        $"{nameof(SendAsync)}: Couldnt get a new token for user = {currentLoggedUsername}. " +
                        $"Error = {tokenResponse.Message}");
                    throw new Exception(tokenResponse.Message);
                }

                var newToken = tokenResponse.Result;
                // we're now logged in again.
                // Save the user to the app settings

                _logger.Information(
                    $"{nameof(SendAsync)}: Saving the new token and refresh token for user = {currentLoggedUsername}");

                userCredentialService.UpdateUserCredential(
                    ResourceType.REFRESH_TOKEN_RESOURCE,
                    currentLoggedUsername,
                    false,
                    newToken.RefreshToken);
                userCredentialService.UpdateUserCredential(
                    ResourceType.TOKEN_RESOURCE,
                    currentLoggedUsername,
                    false,
                    newToken.AccessToken);

                // Clone the request
                clonedRequest = await CloneRequest(request);

                // Set the authentication header
                clonedRequest.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, newToken.AccessToken);
                // Resend the request
                response = await base.SendAsync(clonedRequest, cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // user cancelled auth, so lets return the original response
                return response;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{nameof(SendAsync)}: An unkown error occurred while trying to send request");
                Debugger.Break();
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