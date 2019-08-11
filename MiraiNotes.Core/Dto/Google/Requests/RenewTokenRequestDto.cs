using Refit;

namespace MiraiNotes.Core.Dto.Google.Requests
{
    public class RenewTokenRequestDto
    {
        [AliasAs("refresh_token")]
        public string RefreshToken { get; set; }
        
        [AliasAs("client_id")]
        public string ClientId { get; set; }
        
        [AliasAs("client_secret")]
        public string ClientSecret { get; set; }
        
        [AliasAs("grant_type")]
        public string GrantType { get; set; }
    }
}