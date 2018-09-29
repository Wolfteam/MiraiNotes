using Newtonsoft.Json;

namespace MiraiNotes.UWP.Models.API
{
    public class GoogleBaseModel
    {
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        [JsonProperty(PropertyName = "etag")]
        public string ETag { get; set; }
    }
}
