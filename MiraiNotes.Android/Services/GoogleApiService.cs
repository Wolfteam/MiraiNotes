using System.Threading.Tasks;
using MiraiNotes.Abstractions.GoogleApi;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Dto.Google.Responses;
using MiraiNotes.Shared.Services;
using Serilog;

namespace MiraiNotes.Android.Services
{
    public class GoogleApiService : BaseGoogleApiService
    {
        public GoogleApiService(
            IGoogleApi googleApi,
            ITelemetryService telemetryService,
            ILogger logger) : base(googleApi, telemetryService, logger.ForContext<GoogleApiService>())
        {
        }

        public override Task<ResponseDto<TokenResponseDto>> SignInWithGoogle()
        {
            throw new System.NotImplementedException();
        }
    }
}