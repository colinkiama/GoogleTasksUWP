using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTasksUWPAPI
{
    public sealed class GTaskStatus
    {
        public static string Completed { get; } = "completed";
        public static string NeedsAction { get; } = "needsAction";

        private GTaskStatus() { }
    }
}
