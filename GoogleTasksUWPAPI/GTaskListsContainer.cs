using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GoogleTasksUWPAPI
{
    public sealed class GTaskListsContainer
    {
        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("etag")]
        public string ETag { get; set; }

        [JsonProperty("nextPageToken")]
        public string NextPageToken { get; set; }

        [JsonProperty("items")]
        public GTaskList[] Items { get; set; }
    }
}
