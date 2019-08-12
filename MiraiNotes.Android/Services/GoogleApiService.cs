using System.Threading.Tasks;
using MiraiNotes.Abstractions.GoogleApi;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Dto.Google.Responses;
using MiraiNotes.Shared.Services;

namespace MiraiNotes.Android.Services
{
    public class GoogleApiService : BaseGoogleApiService
    {
        public GoogleApiService(
            IGoogleApi googleApiService, 
            string clientId, 
            string clientSecret, 
            string redirectUrl) : base(googleApiService, clientId, clientSecret, redirectUrl)
        {
        }

        public override Task<ResponseDto<TokenResponseDto>> SignInWithGoogle()
        {
            throw new System.NotImplementedException();
        }
    }
}