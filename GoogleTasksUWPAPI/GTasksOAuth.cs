using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace GoogleTasksUWPAPI
{
    public sealed class GTasksOAuth
    {
        /// <summary>
        ///     OAuth 2.0 client configuration.
        /// </summary>
        private readonly string _clientId;
        private readonly string _redirectUri;

        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        private const string TaskApiScopes = "openid%20profile%20email";

        private const string AccessTokenJsonName = "access_token";
        private const string ExpiresInJsonName = "expires_in";
        private const string RefreshTokenJsonName = "refresh_token";


        public event TokenEventHandler TokenGenerated;

        public GTasksOAuth(string clientId, string redirectUri)
        {
            _clientId = clientId;
            _redirectUri = redirectUri;
        }


        /// <summary>
        ///     Starts an OAuth 2.0 Authorization Request.
        /// </summary>
        public IAsyncAction StartAuthorisationRequestActionAsync()
        {
            return StartAuthorisationRequestAsync().AsAsyncAction();
        }

        /// <summary>
        ///     Starts an OAuth 2.0 Authorization Request.
        /// </summary>
        internal async Task StartAuthorisationRequestAsync()
        {
            // Generates state and PKCE values.
            string state = RandomDataBase64Url(32);
            string codeVerifier = RandomDataBase64Url(32);
            string codeChallenge = Base64UrlEncodeNoPadding(Sha256(codeVerifier));
            const string codeChallengeMethod = "S256";

            // Stores the state and code_verifier values into local settings.
            // Member variables of this class may not be present when the app is resumed with the
            // authorization response, so LocalSettings can be used to persist any needed values.
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["state"] = state;
            localSettings.Values["code_verifier"] = codeVerifier;

            // Creates the OAuth 2.0 authorization request.
            var authorizationRequest =
                $"{AuthorizationEndpoint}?response_type=code&scope={TaskApiScopes}&redirect_uri=" +
                $"{Uri.EscapeDataString(_redirectUri)}&client_id={_clientId}&state={state}&code_challenge={codeChallenge}&code_challenge_method" +
                $"={codeChallengeMethod}";

            Output("Opening authorization request URI: " + authorizationRequest);

            // Opens the Authorization URI in the browser.
            await Launcher.LaunchUriAsync(new Uri(authorizationRequest));


        }

        /// <summary>
        ///     Processes the OAuth 2.0 Authorization Response
        /// </summary>
        /// <param name="e"></param>
        public void HandleUriCallback(NavigationEventArgs e)
        {
            if (e.Parameter is Uri authorizationResponse)
            {
                // Gets URI from navigation parameters.
                var queryString = authorizationResponse.Query;
                Output("MainPage received authorizationResponse: " + authorizationResponse);

                // Parses URI params into a dictionary
                // ref: http://stackoverflow.com/a/11957114/72176
                var queryStringParams = queryString.Substring(1)
                    .Split('&')
                    .ToDictionary(c => c.Split('=')[0], c => Uri.UnescapeDataString(c.Split('=')[1]));

                if (queryStringParams.ContainsKey("error"))
                {
                    Output($"OAuth authorization error: {queryStringParams["error"]}.");
                    return;
                }

                if (!queryStringParams.ContainsKey("code") || !queryStringParams.ContainsKey("state"))
                {
                    Output("Malformed authorization response. " + queryString);
                    return;
                }

                // Gets the Authorization code & state
                var code = queryStringParams["code"];
                var incomingState = queryStringParams["state"];

                // Retrieves the expected 'state' value from local settings (saved when the request was made).
                var localSettings = ApplicationData.Current.LocalSettings;
                var expectedState = (string)localSettings.Values["state"];


                // Compares the received state to the expected value, to ensure that
                // this app made the request which resulted in authorization
                if (incomingState != expectedState)
                {
                    Output($"Received request with invalid state ({incomingState})");
                    return;
                }

                // Resets expected state value to avoid a replay attack.
                localSettings.Values["state"] = null;

                // Authorization Code is now ready to use!
                Output(Environment.NewLine + "Authorization code: " + code);

                var codeVerifier = (string)localSettings.Values["code_verifier"];
                PerformCodeExchangeAsync(code, codeVerifier);

            }
            else
            {
                Debug.WriteLine(e.Parameter);
            }
        }

        private async void PerformCodeExchangeAsync(string code, string codeVerifier)
        {
            // Builds the Token request
            var tokenRequestBody =
                $"code={code}&redirect_uri={Uri.EscapeDataString(_redirectUri)}&client_id={_clientId}" +
                $"&code_verifier={codeVerifier}&scope=&grant_type=authorization_code";
            var content = new StringContent(tokenRequestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

            // Performs the authorization code exchange.
            var handler = new HttpClientHandler();
            handler.AllowAutoRedirect = true;
            var client = new HttpClient(handler);

            Output(Environment.NewLine + "Exchanging code for tokens...");
            var response = await client.PostAsync(TokenEndpoint, content);
            var responseString = await response.Content.ReadAsStringAsync();
            Output(responseString);

            if (!response.IsSuccessStatusCode)
            {
                Output("Authorization code exchange failed.");
                return;
            }

            // Sets the Authentication header of our HTTP client using the acquired access token.
            var tokens = JsonObject.Parse(responseString);
            var accessToken = tokens.GetNamedString("access_token");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            GenerateTokenObject(tokens);

            // Makes a call to the Userinfo endpoint, and prints the results.
            Output("Making API Call to Userinfo...");
            var userinfoResponse = client.GetAsync(UserInfoEndpoint).Result;
            var userinfoResponseContent = await userinfoResponse.Content.ReadAsStringAsync();
            Output(userinfoResponseContent);
        }

        private void GenerateTokenObject(JsonObject tokens)
        {
            var accessToken = tokens.GetNamedString(AccessTokenJsonName);
            var refreshToken = tokens.GetNamedString(RefreshTokenJsonName);
            var expiresIn = tokens.GetNamedNumber(ExpiresInJsonName);

            Token freshToken = new Token(accessToken, refreshToken, expiresIn);

            TokenGenerated?.Invoke(new TokenEventArgs(freshToken));
        }

        /// <summary>
        ///     Appends the given string to the on-screen log, and the debug console.
        /// </summary>
        /// <param name="output">string to be appended</param>
        public void Output(string output)
        {
            Debug.WriteLine(output);
        }

        /// <summary>
        ///     Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string RandomDataBase64Url(uint length)
        {
            var buffer = CryptographicBuffer.GenerateRandom(length);
            return Base64UrlEncodeNoPadding(buffer);
        }

        /// <summary>
        ///     Returns the SHA256 hash of the input string.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static IBuffer Sha256(string inputString)
        {
            var sha = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha256);
            var buff = CryptographicBuffer.ConvertStringToBinary(inputString, BinaryStringEncoding.Utf8);
            return sha.HashData(buff);
        }

        /// <summary>
        ///     Base64url no-padding encodes the given input buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string Base64UrlEncodeNoPadding(IBuffer buffer)
        {
            var base64 = CryptographicBuffer.EncodeToBase64String(buffer);

            // Converts base64 to base64url.
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            // Strips padding.
            base64 = base64.Replace("=", "");

            return base64;
        }
    }
}