using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace MiraiNotes.UWP.Models.API
{
    public class GoogleTaskModel : GoogleBaseModel
    {
        [JsonProperty(PropertyName = "id")]
        public string TaskID { get; set; }

        [Required]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "updated")]
        public DateTime UpdatedAt { get; set; }

        public string SelfLink { get; set; }

        [JsonProperty(PropertyName = "parent")]
        public string ParentTask { get; set; }

        public string Position { get; set; }

        public string Notes { get; set; }

        public string Status { get; set; }

        [JsonProperty(PropertyName = "due")]
        public DateTime? ToBeCompletedOn { get; set; }

        [JsonProperty(PropertyName = "completed")]
        public DateTime? CompletedOn { get; set; }

        [JsonProperty(PropertyName = "deleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty(PropertyName = "hidden")]
        public bool IsHidden { get; set; }
    }
}
