using Newtonsoft.Json;

namespace MiraiNotes.Core.Models.GoogleApi
{
    public class GoogleBaseModel
    {
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        [JsonProperty(PropertyName = "etag")]
        public string ETag { get; set; }
    }
}
