using System;
using Newtonsoft.Json;

namespace MiraiNotes.Core.Models.GoogleApi
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
