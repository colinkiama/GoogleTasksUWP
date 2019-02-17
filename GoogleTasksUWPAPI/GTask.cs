using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTasksUWPAPI
{
    public sealed class GTask
    {
        public string Kind { get; set; } = "tasks#task";
        public string Id { get; set; }
        public string ETag { get; set; }
        public string Title { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public string SelfLink { get; set; }
        public string Parent { get; private set; }
        public string Position { get; private set; }
        public string Notes { get; set; }
        public string Status { get; set; }
        public DateTimeOffset Due { get; set; }
        public bool Completed { get; set; }
        public bool Deleted { get; set; }
        public bool Hidden { get; set; }
        public Link[] Links { get; set; }
    }

   


}
