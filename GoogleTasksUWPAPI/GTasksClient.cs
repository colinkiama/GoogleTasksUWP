using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleTasksUWPAPI
{
    public sealed class GTasksClient
    {
        readonly Uri _tasksEndpointUri = new Uri("https://www.googleapis.com/tasks/v1");

        public GTasksClient(Token token)
        {

        }
    }
}
