using GalaSoft.MvvmLight.Ioc;
using MiraiNotes.UWP.Handlers;
using MiraiNotes.UWP.Interfaces;
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
            var token = _userCredentialService.GetUserToken();
            if (token == null)
                return _httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            return _httpClient;
        }
    }
}