using System;
using System.Diagnostics;
using System.Linq;
using Windows.Web.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http.Headers;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;

namespace GoogleTasksUWPAPI
{
    public sealed class GTasksOAuth
    {
        /// <summary>
        /// Modified version of Google's OAuth for Universal App
        /// example: https://github.com/googlesamples/oauth-apps-for-windows/tree/master/OAuthUniversalApp
        /// </summary>
        private readonly string _clientId;
        private readonly string _redirectUri;

        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
        private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";
        private const string TaskApiScopes = "openid%20email%20profile%20https://www.googleapis.com/auth/tasks%20profile";

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
        public IAsyncOperation<Token> HandleUriCallbackAsync(NavigationEventArgs e)
        {
            return HandleUriCallbackTask(e).AsAsyncOperation();
        }

        /// <summary>
        ///     Processes the OAuth 2.0 Authorization Response
        /// </summary>
        /// <param name="e"></param>
        internal async Task<Token> HandleUriCallbackTask(NavigationEventArgs e)
        {
            Token tokenToGenerate = new Token();
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
                    return tokenToGenerate;
                }

                if (!queryStringParams.ContainsKey("code") || !queryStringParams.ContainsKey("state"))
                {
                    Output("Malformed authorization response. " + queryString);
                    return tokenToGenerate;
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
                    return tokenToGenerate;
                }

                // Resets expected state value to avoid a replay attack.
                localSettings.Values["state"] = null;

                // Authorization Code is now ready to use!
                Output(Environment.NewLine + "Authorization code: " + code);

                var codeVerifier = (string)localSettings.Values["code_verifier"];
                tokenToGenerate = await PerformCodeExchangeAsync(code, codeVerifier);

            }
            else
            {
                Debug.WriteLine(e.Parameter);
            }

            return tokenToGenerate;
        }

        private async Task<Token> PerformCodeExchangeAsync(string code, string codeVerifier)
        {
            Token tokenToReturn = new Token();
            // Builds the Token request
            var tokenRequestBody =
                $"code={code}&redirect_uri={Uri.EscapeDataString(_redirectUri)}&client_id={_clientId}" +
                $"&code_verifier={codeVerifier}&scope=&grant_type=authorization_code";
            var content = new HttpStringContent(tokenRequestBody, UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");

            // Performs the authorization code exchange.
            var client = new HttpClient();

            Output(Environment.NewLine + "Exchanging code for tokens...");
            var response = await client.PostAsync(new Uri(TokenEndpoint), content);
            var responseString = await response.Content.ReadAsStringAsync();
            Output(responseString);

            if (!response.IsSuccessStatusCode)
            {
                Output("Authorization code exchange failed.");                
            }

            else
            { 
                // Sets the Authentication header of our HTTP client using the acquired access token.
                var tokens = JsonObject.Parse(responseString);
                var accessToken = tokens.GetNamedString("access_token");
                client.DefaultRequestHeaders.Authorization = new HttpCredentialsHeaderValue("Bearer", accessToken);

                tokenToReturn = GenerateTokenObject(tokens);

                // Makes a call to the Userinfo endpoint, and prints the results.
                Output("Making API Call to Userinfo...");
                var userinfoResponse = await client.GetAsync(new Uri(UserInfoEndpoint));
                var userinfoResponseContent = await userinfoResponse.Content.ReadAsStringAsync();
                Output(userinfoResponseContent);
            }
            return tokenToReturn;
        }


        private Token GenerateTokenObject(JsonObject tokens, string previousRefreshToken = "")
        {
            var accessToken = tokens.GetNamedString(AccessTokenJsonName);
            var refreshToken = tokens.GetNamedString(RefreshTokenJsonName,"");
            if (refreshToken == "")
            {
                refreshToken = previousRefreshToken;
            }

            var expiresIn = tokens.GetNamedNumber(ExpiresInJsonName);

            Token freshToken = new Token(accessToken, refreshToken, expiresIn);
            TokenGenerated?.Invoke(new TokenEventArgs(freshToken));
            return freshToken;
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


        public IAsyncOperation<Token> TokenRefreshAsync(Token tokenData)
        {
            return TokenRefreshTask(tokenData).AsAsyncOperation();
        }

        internal async Task<Token> TokenRefreshTask(Token tokenData)
        {
            Token generatedToken = new Token();
            var tokenRefreshBody =
                $"grant_type=refresh_token&refresh_token={tokenData.RefreshToken}&" +
                $"client_id={_clientId}&bundle_id={_redirectUri}";

            var content = new HttpStringContent(tokenRefreshBody, UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");

            // Performs the authorization code exchange.
            var client = new HttpClient();

            Output(Environment.NewLine + "Exchanging code for tokens...");
            var response = await client.PostAsync(new Uri( TokenEndpoint), content);
            var responseString = await response.Content.ReadAsStringAsync();
            Output(responseString);
            if (response.IsSuccessStatusCode)
            {
                var tokenJson = JsonObject.Parse(responseString);
               generatedToken = GenerateTokenObject(tokenJson, tokenData.RefreshToken);
            }

            else
            {
                Debug.WriteLine("Failed");
            }

            return generatedToken;
        }
    }
}