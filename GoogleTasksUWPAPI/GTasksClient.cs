using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Windows.Web.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Web.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Windows.Storage.Streams;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;


namespace GoogleTasksUWPAPI
{
    public sealed class GTasksClient
    {
        private readonly Uri _tasksEndpointUri = new Uri("https://www.googleapis.com/tasks/v1");
        private HttpClient _client;
        private const string TokenScheme = "Bearer";
        private const string JsonMediaType = "application/json";
        private const string PatchHttpMethod = "PATCH";



        public Token Token { get; set; }              


        public GTasksClient(Token token)
        {
            Token = token;
            _client = new HttpClient();
        }

        #region Tasklists


        public IAsyncOperation<GTaskList> PatchTaskListAsync(string taskListId)
        {
            return PatchTaskListTask(taskListId).AsAsyncOperation();
        }

        private async Task<GTaskList> PatchTaskListTask(string taskListId)
        {
            GTaskList listToReturn = null;
            var requestUri = new Uri($"https://www.googleapis.com/tasks/v1/users/@me/lists/{taskListId}");

            var request = new HttpRequestMessage(HttpMethod.Patch, requestUri);
            

            var responseMessage = await _client.SendRequestAsync(request);

            if (responseMessage.IsSuccessStatusCode)
            {
                
            }
            return listToReturn;
        }

        public IAsyncOperation<bool> DeleteTaskListAsync(string taskListId)
        {
            return DeleteTaskListTask(taskListId).AsAsyncOperation();
        }

        private async Task<bool> DeleteTaskListTask(string taskListId)
        {
            bool isTaskListDeleted;
            var requestUri = new Uri($"https://www.googleapis.com/tasks/v1/users/@me/lists/{taskListId}");
            AddTokenInHeader(_client);

            var responseMessage = await _client.DeleteAsync(requestUri);
            isTaskListDeleted = responseMessage.IsSuccessStatusCode;
            return isTaskListDeleted;
        }

        public IAsyncOperation<GTaskList> UpdateTaskListAsync(GTaskList listToUpdate)
        {
            return UpdateTaskListTask(listToUpdate).AsAsyncOperation();
        }

        private async Task<GTaskList> UpdateTaskListTask(GTaskList listToUpdate)
        {
            GTaskList listToReturn = null;

            var requestUri = new Uri($"https://www.googleapis.com/tasks/v1/users/@me/lists/{listToUpdate.Id}");

            AddTokenInHeader(_client);

            var taskListJson = JsonConvert.SerializeObject(listToUpdate);

            var content = new HttpStringContent(taskListJson, UnicodeEncoding.Utf8, JsonMediaType);

            var responseMessage = await _client.PutAsync(requestUri, content);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                listToReturn = JsonConvert.DeserializeObject<GTaskList>(responseJson);
            }

            return listToReturn;
        }

        public IAsyncOperation<GTaskList> InsertTaskListAsync(GTaskList listToInsert)
        {
            return InsertTaskListTask(listToInsert).AsAsyncOperation();
        }

        private async Task<GTaskList> InsertTaskListTask(GTaskList listToInsert)
        {
            GTaskList listToReturn = null;
            var requestUri = new Uri("https://www.googleapis.com/tasks/v1/users/@me/lists");

            AddTokenInHeader(_client);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            var taskListJson = JsonConvert.SerializeObject(listToInsert);

            var content = new HttpStringContent(taskListJson, UnicodeEncoding.Utf8, JsonMediaType);
            var responseMessage = await _client.PostAsync(requestUri, content);

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                listToReturn =  JsonConvert.DeserializeObject<GTaskList>(responseJson);
            }

            return listToReturn;
        }

        private void AddTokenInHeader(HttpClient client)
        {
            _client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue(TokenScheme, Token.AccessToken);

        }


        public IAsyncOperation<GTaskList> GetTaskListAsync(string taskListId)
        {
           return GetTaskListTask(taskListId).AsAsyncOperation();
        }

        internal async Task<GTaskList> GetTaskListTask(string taskListId)
        {
            GTaskList taskList = null;
            var requestUri = new Uri($"https://www.googleapis.com/tasks/v1/users/@me/lists/{taskListId}");

            AddTokenInHeader(_client);

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
            var requestUri = new Uri("https://www.googleapis.com/tasks/v1/users/@me/lists");
            GTaskListsContainer taskListsContainer = null;

            AddTokenInHeader(_client);

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
