using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GoogleTasksUWPAPI
{
    public sealed class GTasksClient
    {
        private readonly Uri _tasksEndpointUri = new Uri("https://www.googleapis.com/tasks/v1");
        private HttpClient _client;
        private const string TokenScheme = "Bearer";
        public Token Token { get; set; }              


        public GTasksClient(Token token)
        {
            Token = token;
            _client = new HttpClient();
        }

        #region Tasklists

        public IAsyncOperation<GTaskList> InsertTaskListAsync(GTaskList listToInsert)
        {
            return InsertTaskListTask(listToInsert).AsAsyncOperation();
        }

        private async Task<GTaskList> InsertTaskListTask(GTaskList listToInsert)
        {
            GTaskList listToReturn = null;
            var requestUri = "https://www.googleapis.com/tasks/v1/users/@me/lists";

            return listToReturn;
        }


        public IAsyncOperation<GTaskList> GetTaskListAsync(string taskListId)
        {
           return GetTaskListTask(taskListId).AsAsyncOperation();
        }

        internal async Task<GTaskList> GetTaskListTask(string taskListId)
        {
            GTaskList taskList = null;
            var requestUri = $"https://www.googleapis.com/tasks/v1/users/@me/lists/{taskListId}";

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TokenScheme, TokenScheme);

            var responseMessage = await _client.GetAsync(requestUri);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                taskList = JsonConvert.DeserializeObject<GTaskList>(responseJson);
            }

            return taskList;
        }

        public IAsyncOperation<GTaskListsContainer> ListTaskListsAsync()
        {
            return ListTaskListsTask().AsAsyncOperation();
        }

        internal async Task<GTaskListsContainer> ListTaskListsTask()
        {
            const string requestUri = "https://www.googleapis.com/tasks/v1/users/@me/lists";
            GTaskListsContainer taskListsContainer = null;

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TokenScheme, Token.AccessToken);

            var responseMessage = await _client.GetAsync(requestUri);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                taskListsContainer = JsonConvert.DeserializeObject<GTaskListsContainer>(responseJson);
            }
            return taskListsContainer;
        }
        #endregion

        #region Tasks

        
        #endregion

    }
}
