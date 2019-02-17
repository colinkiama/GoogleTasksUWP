using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GoogleTasksUWPAPI
{
    public sealed class Link
    {
        [JsonProperty("links[].type")]
        public string Type { get; set; }

        [JsonProperty("links[].description")]
        public string Description { get; set; }

        [JsonProperty("links[].link")]
        public string LinkString { get; set; }
    }
}
