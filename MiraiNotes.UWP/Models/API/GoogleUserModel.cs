using Newtonsoft.Json;

namespace MiraiNotes.UWP.Models
{
    public class GoogleUserModel
    {
        [JsonProperty(PropertyName = "sub")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "picture")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string FullName { get; set; }
    }
}
