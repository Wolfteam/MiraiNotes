using System.Threading.Tasks;
using MiraiNotes.Abstractions.GoogleApi;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Dto.Google.Responses;
using MiraiNotes.Shared.Services;

namespace MiraiNotes.Android.Services
{
    public class GoogleApiService : BaseGoogleApiService
    {
        public GoogleApiService(
            IGoogleApi googleApi, 
            ITelemetryService telemetryService,
            string clientId, 
            string clientSecret, 
            string redirectUrl) : base(googleApi, telemetryService, clientId, clientSecret, redirectUrl)
        {
        }

        public override Task<ResponseDto<TokenResponseDto>> SignInWithGoogle()
        {
            throw new System.NotImplementedException();
        }
    }
}