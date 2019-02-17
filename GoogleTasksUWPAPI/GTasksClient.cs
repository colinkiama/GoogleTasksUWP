﻿using System;
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

        private const string TokenScheme = "Bearer";
        public Token Token { get; set; }              


        public GTasksClient(Token token)
        {
            Token = token;
        }

        #region Tasklists

        public IAsyncOperation<GTaskListsContainer> ListTaskListsAsync()
        {
            return ListTaskListsTask().AsAsyncOperation();
        }

        internal async Task<GTaskListsContainer> ListTaskListsTask()
        {
            const string requestUri = "https://www.googleapis.com/tasks/v1/users/@me/lists";
            GTaskListsContainer taskListsContainer = new GTaskListsContainer();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TokenScheme, Token.AccessToken);

            var responseMessage = await client.GetAsync(requestUri);

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