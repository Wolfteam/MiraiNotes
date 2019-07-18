using Refit;

namespace MiraiNotes.Shared.Dto.Google.Requests
{
    public class TokenRequestDto
    {
        [AliasAs("code")]
        public string ApprovalCode { get; set; }
        
        [AliasAs("client_id")]
        public string ClientId { get; set; }
        
        [AliasAs("client_secret")]
        public string ClientSecret { get; set; }
        
        [AliasAs("redirect_uri")]
        public string RedirectUri { get; set; }
        
        [AliasAs("grant_type")]
        public string GrantType { get; set; }
    }
}