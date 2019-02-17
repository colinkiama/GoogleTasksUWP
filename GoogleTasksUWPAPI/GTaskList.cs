using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTasksUWPAPI
{
    public sealed class GTaskList
    {
        public string Kind { get; set; }
        public string Id { get; set; }
        public string ETag { get; set; }
        public string Title { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string SelfLink { get; set; }
    }
}
