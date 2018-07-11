using Newtonsoft.Json;

namespace MiraiNotes.UWP.Models.API
{
    public class GoogleResponseErrorModel
    {
        [JsonProperty(PropertyName = "error_description")]
        public string ErrorDescription { get; set; }

        [JsonProperty(PropertyName = "error")]
        public GoogleApiErrorModel ApiError { get; set; }
    }
}
