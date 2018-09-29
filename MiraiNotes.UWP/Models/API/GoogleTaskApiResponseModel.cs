using Newtonsoft.Json;
using System.Collections.Generic;

namespace MiraiNotes.UWP.Models.API
{
    public class GoogleTaskApiResponseModel<T> : GoogleBaseModel where T : class
    {
        public string NextPageToken { get; set; }

        //[JsonProperty(PropertyName = "items")]
        public IEnumerable<T> Items { get; set; }
    }
}
