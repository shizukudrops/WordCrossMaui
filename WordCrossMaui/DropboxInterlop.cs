using Dropbox.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Dropbox.Api.Files;
using static Dropbox.Api.Files.SearchMatchTypeV2;

namespace WordCrossMaui
{
    internal class DropboxInterlop
    {
        // Add an ApiKey (from https://www.dropbox.com/developers/apps) here
        private readonly string ApiKey = Keys.DropboxApiKey;

        // Add an ApiSecret (from https://www.dropbox.com/developers/apps) here
        private readonly string ApiSecret = Keys.DropboxApiSecret;

        // This loopback host is for demo purpose. If this port is not
        // available on your machine you need to update this URL with an unused port.
        private const string LoopbackHost = "http://127.0.0.1:8080/";

        // URL to receive OAuth 2 redirect from Dropbox server.
        // You also need to register this redirect URL on https://www.dropbox.com/developers/apps.
        private readonly Uri RedirectUri = new Uri(LoopbackHost + "authorize");

        // URL to receive access token from JS.
        private readonly Uri JSRedirectUri = new Uri(LoopbackHost + "token");

        public async Task<bool> Run()
        {
            DropboxCertHelper.InitializeCertPinning();

            string[] scopeList = new string[5] { "files.metadata.read", "files.metadata.write", "files.content.read", "files.content.write", "account_info.read" };

            await AcquireAccessToken(scopeList, IncludeGrantedScopes.None);

            try
            {
                var accessToken = Preferences.Get("dropbox_access_token", null);
                var refreshToken = Preferences.Get("dropbox_refresh_token", null);

                var config = new DropboxClientConfig("WordCross");

                var client = new DropboxClient(accessToken, refreshToken, ApiKey, ApiSecret, config);

                // This call should succeed since the correct scope has been acquired
                await GetCurrentAccount(client);
                
                using (var response = await client.Files.DownloadAsync("/test.txt"))
                {
                    Debug.WriteLine($"Downloaded {response.Response.Name} Rev {response.Response.Rev}");
                    Debug.WriteLine("------------------------------");
                    Debug.WriteLine(await response.GetContentAsStringAsync());
                    Debug.WriteLine("------------------------------");
                }

                using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes("helloworld")))
                {
                    var response = await client.Files.UploadAsync("/testup.txt", WriteMode.Overwrite.Instance, body: stream);

                    Debug.WriteLine($"Uploaded Id {response.Id} Rev {response.Rev}");
                }

                Debug.WriteLine("Oauth Test Complete!");
            }
            catch (HttpException e)
            {
                Debug.WriteLine("Exception reported from RPC layer");
                Debug.WriteLine($"    Status code: {e.StatusCode}");
                Debug.WriteLine($"    Message    : {e.Message}");

                if (e.RequestUri != null)
                {
                    Debug.WriteLine($"    Request uri: {e.RequestUri}");
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles the redirect from Dropbox server. Because we are using token flow, the local
        /// http server cannot directly receive the URL fragment. We need to return a HTML page with
        /// inline JS which can send URL fragment to local server as URL parameter.
        /// </summary>
        /// <param name="http">The http listener.</param>
        /// <returns>The <see cref="Task"/></returns>
        private async Task HandleOAuth2Redirect(HttpListener http)
        {
            var context = await http.GetContextAsync();

            // We only care about request to RedirectUri endpoint.
            while (context.Request.Url.AbsolutePath != RedirectUri.AbsolutePath)
            {
                context = await http.GetContextAsync();
            }

            context.Response.ContentType = "text/html";

            // Respond with a page which runs JS and sends URL fragment as query string
            // to TokenRedirectUri.
            using (var file = await FileSystem.OpenAppPackageFileAsync("OAuthRedirect.html"))
            {
                file.CopyTo(context.Response.OutputStream);
            }

            context.Response.OutputStream.Close();
        }

        /// <summary>
        /// Handle the redirect from JS and process raw redirect URI with fragment to
        /// complete the authorization flow.
        /// </summary>
        /// <param name="http">The http listener.</param>
        /// <returns>The <see cref="OAuth2Response"/></returns>
        private async Task<Uri> HandleJSRedirect(HttpListener http)
        {
            var context = await http.GetContextAsync();

            // We only care about request to TokenRedirectUri endpoint.
            while (context.Request.Url.AbsolutePath != JSRedirectUri.AbsolutePath)
            {
                context = await http.GetContextAsync();
            }

            var redirectUri = new Uri(context.Request.QueryString["url_with_fragment"]);

            return redirectUri;
        }

        /// <summary>
        /// Acquires a dropbox access token and saves it to the default Preferences for the app.
        /// <para>
        /// This fetches the access token from the applications Preferences, if it is not found there
        /// (or if the user chooses to reset the Preferences) then the UI in <see cref="LoginForm"/> is
        /// displayed to authorize the user.
        /// </para>
        /// </summary>
        /// <returns>A valid uid if a token was acquired or null.</returns>
        private async Task<string> AcquireAccessToken(string[] scopeList, IncludeGrantedScopes includeGrantedScopes)
        {
            var accessToken = Preferences.Get("dropbox_access_token", null);
            var refreshToken = Preferences.Get("dropbox_refresh_token", null);

            if (string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    Debug.WriteLine("Waiting for credentials.");
                    var state = Guid.NewGuid().ToString("N");

                    var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, ApiKey, RedirectUri, state: state, tokenAccessType: TokenAccessType.Offline, scopeList: scopeList, includeGrantedScopes: includeGrantedScopes);
                    
                    var http = new HttpListener();
                    
                    http.Prefixes.Add(LoopbackHost);

                    http.Start();

                    await Browser.Default.OpenAsync(authorizeUri, BrowserLaunchMode.SystemPreferred);

                    // Handle OAuth redirect and send URL fragment to local server using JS.
                    await HandleOAuth2Redirect(http);

                    // Handle redirect from JS and process OAuth response.
                    var redirectUri = await HandleJSRedirect(http);

                    Debug.WriteLine("Exchanging code for token");

                    var tokenResult = await DropboxOAuth2Helper.ProcessCodeFlowAsync(redirectUri, ApiKey, ApiSecret, RedirectUri.ToString(), state);

                    Debug.WriteLine("Finished Exchanging Code for Token");

                    accessToken = tokenResult.AccessToken;
                    refreshToken = tokenResult.RefreshToken;

                    var uid = tokenResult.Uid;

                    if (tokenResult.RefreshToken != null)
                    {
                        Preferences.Default.Set("dropbox_refresh_token", refreshToken);
                    }

                    Preferences.Default.Set("dropbox_access_token", accessToken);

                    http.Stop();

                    return uid;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error: {e.Message}");
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets information about the currently authorized account.
        /// <para>
        /// This demonstrates calling a simple rpc style api from the Users namespace.
        /// </para>
        /// </summary>
        /// <param name="client">The Dropbox client.</param>
        /// <returns>An asynchronous task.</returns>
        private async Task GetCurrentAccount(DropboxClient client)
        {
            try
            {
                Debug.WriteLine("Current Account:");
                var full = await client.Users.GetCurrentAccountAsync();

                Debug.WriteLine($"Account id    : {full.AccountId}");
                Debug.WriteLine($"Country       : {full.Country}");
                Debug.WriteLine($"Email         : {full.Email}");
                Debug.WriteLine($"Is paired     : {full.IsPaired}");
                Debug.WriteLine($"Locale        : {full.Locale}");
                Debug.WriteLine("Name");
                Debug.WriteLine($"  Display  : {full.Name.DisplayName}");
                Debug.WriteLine($"  Familiar : {full.Name.FamiliarName}");
                Debug.WriteLine($"  Given    : {full.Name.GivenName}");
                Debug.WriteLine($"  Surname  : {full.Name.Surname}");
                Debug.WriteLine($"Referral link : {full.ReferralLink}");

                if (full.Team != null)
                {
                    Debug.WriteLine("Team");
                    Debug.WriteLine($"  Id   : {full.Team.Id}");
                    Debug.WriteLine($"  Name : {full.Team.Name}");
                }
                else
                {
                    Debug.WriteLine("Team - None");
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
}

