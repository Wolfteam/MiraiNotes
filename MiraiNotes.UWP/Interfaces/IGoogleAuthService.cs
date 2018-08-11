﻿using MiraiNotes.UWP.Models;
using System.Threading.Tasks;

namespace MiraiNotes.UWP.Interfaces
{
    public interface IGoogleAuthService : IProvider
    {
        string GetAuthorizationUrl();
        Task<TokenResponse> GetTokenAsync(string approvalCode);
        Task<TokenResponse> GetNewTokenAsync(string refreshToken);
    }
}