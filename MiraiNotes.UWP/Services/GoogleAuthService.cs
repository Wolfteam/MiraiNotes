using MiraiNotes.Shared.Models;
using MiraiNotes.UWP.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web;

namespace MiraiNotes.UWP.Services
{
    public class GoogleAuthService : BaseGoogleAuthService
    {
        public override async Task<Response<TokenResponse>> SignInWithGoogle()
        {
            var requestUri = new Uri(GetAuthorizationUrl());
            var callbackUri = new Uri(ApprovalUrl);
            var response = new Response<TokenResponse>();
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
                        string approvalCode = queryParams["approvalCode"];
                        var tokenResponse = await GetTokenAsync(approvalCode);
                        if (tokenResponse == null)
                        {
                            response.Message = "Couldn't get a token";
                            break;
                        }

                        response.Result = tokenResponse;
                        response.Succeed = true;
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