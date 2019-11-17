using Newtonsoft.Json;

namespace MiraiNotes.Core.Models.GoogleApi
{
    public class GoogleResponseErrorModel
    {
        [JsonProperty(PropertyName = "error_description")]
        public string ErrorDescription { get; set; }

        [JsonProperty(PropertyName = "error")]
        public GoogleApiErrorModel ApiError { get; set; }
    }
}
