using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GoogleTasksUWPAPI
{
    [JsonObject]
    public sealed class GTaskList
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("etag")]
        public string ETag { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("selfLink")]
        public string SelfLink { get; set; }

        [JsonProperty("updated")]
        public DateTimeOffset Updated { get; set; }
    }
}
