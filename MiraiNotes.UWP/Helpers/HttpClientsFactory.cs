using GalaSoft.MvvmLight.Ioc;
using MiraiNotes.UWP.Handlers;
using MiraiNotes.UWP.Interfaces;
using MiraiNotes.UWP.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MiraiNotes.UWP.Helpers
{
    public class HttpClientsFactory : IHttpClientsFactory
    {
        private readonly HttpClient _httpClient;
        private readonly IUserCredentialService _userCredentialService;

        public HttpClientsFactory(IUserCredentialService userCredentialService)
        {
            _httpClient = new HttpClient();
            _userCredentialService = userCredentialService;
        }

        [PreferredConstructor]
        public HttpClientsFactory(AuthorizationHandler authorizationHandler, IUserCredentialService userCredentialService)
        {
            _httpClient = new HttpClient(authorizationHandler);
            _userCredentialService = userCredentialService;
        }

        public HttpClient GetHttpClient()
        {
            string currentLoggedUsername = _userCredentialService.GetCurrentLoggedUsername();

            if (string.IsNullOrEmpty(currentLoggedUsername))
                return _httpClient;

            string token = _userCredentialService.GetUserCredential(
                PasswordVaultResourceType.TOKEN_RESOURCE, 
                currentLoggedUsername);

            if (string.IsNullOrEmpty(token))
                return _httpClient;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return _httpClient;
        }
    }
}