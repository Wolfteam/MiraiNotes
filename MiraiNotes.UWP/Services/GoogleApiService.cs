using MiraiNotes.Abstractions.GoogleApi;
using MiraiNotes.Abstractions.Services;
using MiraiNotes.Core.Dto;
using MiraiNotes.Core.Dto.Google.Responses;
using MiraiNotes.Shared;
using MiraiNotes.Shared.Services;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace MiraiNotes.UWP.Services
{
    public class GoogleApiService : BaseGoogleApiService
    {
        public GoogleApiService(
            IGoogleApi googleApi,
            ITelemetryService telemetryService,
            ILogger logger)
            : base(googleApi, telemetryService, logger.ForContext<GoogleApiService>())
        {
        }

        public override async Task<ResponseDto<TokenResponseDto>> SignInWithGoogle()
        {
            var requestUri = new Uri(GetAuthorizationUrl());
            var callbackUri = new Uri(AppConstants.BaseGoogleApprovalUrl);
            var response = new ResponseDto<TokenResponseDto>();
            try
            {
                var result = await WebAuthenticationBroker
                    .AuthenticateAsync(WebAuthenticationOptions.None, requestUri, callbackUri);
                switch (result.ResponseStatus)
                {
                    case WebAuthenticationStatus.Success:
                        var queryParams = result.ResponseData
                            .Split('&')
                            .ToDictionary(
                                c => c.Split('=')[0],
                                c => Uri.UnescapeDataString(c.Split('=')[1]));

                        if (queryParams.ContainsKey("error"))
                        {
                            response.Message = $"OAuth authorization error: {queryParams["error"]}";
                            break;
                        }

                        if (!queryParams.ContainsKey("approvalCode"))
                        {
                            response.Message = "Malformed authorization response. Couldnt get an apporval code";
                            break;
                        }

                        // Gets the Authorization code
                        var approvalCode = queryParams["approvalCode"];
                        var tokenResponse = await GetAccessTokenAsync(approvalCode);

                        response = tokenResponse;
                        break;
                    case WebAuthenticationStatus.UserCancel:
                        break;
                    case WebAuthenticationStatus.ErrorHttp:
                        response.Message = $"Http error, {result.ResponseErrorDetail}";
                        break;
                    default:
                        response.Message = $"{result.ResponseData}";
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Message = $"An unknown error occurred. Ex = {ex.Message}";
            }

            return response;
        }
    }
}
