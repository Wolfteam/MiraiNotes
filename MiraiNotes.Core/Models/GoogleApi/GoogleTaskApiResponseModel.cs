using Newtonsoft.Json;
using System.Collections.Generic;

namespace MiraiNotes.Core.Models.GoogleApi
{
    public class GoogleTaskApiResponseModel<T> : GoogleBaseModel where T : class
    {
        [JsonProperty(PropertyName = "nextPageToken")]
        public string NextPageToken { get; set; }

        [JsonProperty(PropertyName = "items")]
        public IEnumerable<T> Items { get; set; }
    }
}
