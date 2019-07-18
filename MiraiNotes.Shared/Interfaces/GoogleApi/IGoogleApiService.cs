using System.Threading.Tasks;
using MiraiNotes.Shared.Dto.Google.Requests;
using MiraiNotes.Shared.Dto.Google.Responses;
using Refit;

namespace MiraiNotes.Shared.Interfaces.GoogleApi
{
    public interface IGoogleApiService
    {
        [Post("/oauth2/v4/token")]
        Task<TokenResponseDto> GetAccessToken([Body(BodySerializationMethod.UrlEncoded)] TokenRequestDto request);

        [Post("/oauth2/v4/token")]
        Task<TokenResponseDto> RenewToken([Body(BodySerializationMethod.UrlEncoded)] RenewTokenRequestDto request);
    }
}