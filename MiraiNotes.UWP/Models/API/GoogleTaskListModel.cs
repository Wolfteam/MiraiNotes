using Newtonsoft.Json;
using System;

namespace MiraiNotes.UWP.Models.API
{
    public class GoogleTaskListModel : GoogleBaseModel
    {
        [JsonProperty(PropertyName = "id")]
        public string TaskListID { get; set; }

        public string Title { get; set; }

        public string SelfLink { get; set; }

        [JsonProperty(PropertyName = "updated")]
        public DateTime UpdatedAt { get; set; }
    }
}
