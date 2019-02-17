using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GoogleTasksUWPAPI
{
    [JsonObject]
    public sealed class GTask
    {
        [JsonProperty("kind")]
        public string Kind { get; set; } = "tasks#task";

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("etag")]
        public string ETag { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("updated")]
        public DateTimeOffset DateTime { get; set; }

        [JsonProperty("selfLink")]
        public string SelfLink { get; set; }

        [JsonProperty("position")]
        public string Parent { get; private set; }

        [JsonProperty("kind")]
        public string Position { get; private set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("due")]
        public DateTimeOffset Due { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("hidden")]
        public bool Hidden { get; set; }

        [JsonProperty("links[]")]
        public Link[] Links { get; set; }
    }

   


}
