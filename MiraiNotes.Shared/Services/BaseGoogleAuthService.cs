using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MiraiNotes.Shared.Dto.Google.Requests;
using MiraiNotes.Shared.Dto.Google.Responses;
using MiraiNotes.Shared.Interfaces;
using MiraiNotes.Shared.Interfaces.GoogleApi;
using MiraiNotes.Shared.Models;

namespace MiraiNotes.Shared.Services
{
    public abstract class BaseGoogleAuthService : IGoogleAuthService
    {
        private readonly IGoogleApiService _googleApiService;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUrl;
        private readonly IReadOnlyList<string> _scopes = new List<string>
        {
            "https://www.googleapis.com/auth/tasks",
            "https://www.googleapis.com/auth/userinfo.profile",
            "https://www.googleapis.com/auth/userinfo.email"
        };

        private const string GoogleRefreshGrantType = "refresh_token";
        private const string GoogleTokenGrantType = "authorization_code";

        
        public BaseGoogleAuthService(
            IGoogleApiService googleApiService,
            string clientId,
            string clientSecret,
            string redirectUrl)
        {
            _googleApiService = googleApiService;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUrl = redirectUrl;
        }
        
        public string GetAuthorizationUrl()
        {
            return $"{AppConstants.BaseGoogleAuthUrl}" +
                   $"?client_id={Uri.EscapeDataString(_clientId)}" +
                   $"&scope={string.Join(" ", _scopes)}" +
                   $"&redirect_uri={Uri.EscapeDataString(_redirectUrl)}" +
                   "&response_type=code" +
                   "&include_granted_scopes=true";
        }

        public async Task<TokenResponseDto> GetAccessTokenAsync(string approvalCode)
        {
            return await _googleApiService.GetAccessToken(new TokenRequestDto
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                RedirectUri = _redirectUrl,
                GrantType = GoogleTokenGrantType,
                ApprovalCode = approvalCode,
            });
        }

        public async Task<TokenResponseDto> GetNewTokenAsync(string refreshToken)
        {
            return await _googleApiService.RenewToken(new RenewTokenRequestDto
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret,
                GrantType = GoogleRefreshGrantType,
                RefreshToken = refreshToken
            });
        }

        public abstract Task<ResponseDto<TokenResponseDto>> SignInWithGoogle();
    }
}