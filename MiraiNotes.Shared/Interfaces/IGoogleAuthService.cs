using System.Threading.Tasks;
using MiraiNotes.Shared.Models;
using MiraiNotes.Shared.Dto.Google.Responses;

namespace MiraiNotes.Shared.Interfaces
{
    public interface IGoogleAuthService
    {
        string GetAuthorizationUrl();
        Task<TokenResponseDto> GetAccessTokenAsync(string approvalCode);
        Task<TokenResponseDto> GetNewTokenAsync(string refreshToken);
        Task<ResponseDto<TokenResponseDto>> SignInWithGoogle();
    }
}