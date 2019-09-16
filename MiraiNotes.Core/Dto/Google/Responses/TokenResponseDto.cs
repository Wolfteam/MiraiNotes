using Newtonsoft.Json;

namespace MiraiNotes.Core.Dto.Google.Responses
{
    public class TokenResponseDto
    {
        [JsonProperty(PropertyName = "id_token")]
        public string TokenId { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        //If you are renewing a token using the refresh
        //token, then you need to manually set this one
        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }
    }
}
