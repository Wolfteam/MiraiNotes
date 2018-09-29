using Newtonsoft.Json;

namespace MiraiNotes.UWP.Models
{
    public class TokenResponse
    {
        [JsonProperty(PropertyName = "id_token")]
        public string TokenID { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }

        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }
    }
}
