using Newtonsoft.Json;
using System;

namespace MiraiNotes.UWP.Models.API
{
    public class GoogleTaskListModel : GoogleBaseModel
    {
        [JsonProperty(PropertyName = "id")]
        public string TaskListID { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "selfLink")]
        public string SelfLink { get; set; }

        [JsonProperty(PropertyName = "updated")]
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
