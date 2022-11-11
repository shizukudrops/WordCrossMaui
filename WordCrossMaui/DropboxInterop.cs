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

namespace WordCrossMaui
{
    internal class DropboxInterop
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
        private readonly Uri SecondaryRedirectUri = new Uri(LoopbackHost + "token");

        public DropboxInterop() 
        {
            DropboxCertHelper.InitializeCertPinning();
        }

        public async Task<bool> Authenticate()
        {
            string[] scopeList = new string[5] { "files.metadata.read", "files.metadata.write", "files.content.read", "files.content.write", "account_info.read" };

            return !string.IsNullOrEmpty(await AcquireAccessToken(scopeList, IncludeGrantedScopes.None));
        }

        public async Task<bool> IsFileExist(string path, string fileName)
        {
            try
            {
                var accessToken = Preferences.Get("dropbox_access_token", null);
                var refreshToken = Preferences.Get("dropbox_refresh_token", null);

                var config = new DropboxClientConfig("WordCross");

                using (var client = new DropboxClient(accessToken, refreshToken, ApiKey, ApiSecret, config))
                {
                    var list = await client.Files.ListFolderAsync(path);

                    foreach (var item in list.Entries.Where(i => i.IsFile))
                    {
                        var file = item.AsFile;

                        if(file.Name == fileName)
                        {
                            return true;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    return false;
                }
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
        }

        public async Task<string?> Download(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            try
            {
                var accessToken = Preferences.Get("dropbox_access_token", null);
                var refreshToken = Preferences.Get("dropbox_refresh_token", null);

                var config = new DropboxClientConfig("WordCross");

                using (var client = new DropboxClient(accessToken, refreshToken, ApiKey, ApiSecret, config))
                {
                    var response = await client.Files.DownloadAsync(path);                
                    var content = await response.GetContentAsStringAsync();

                    Debug.WriteLine($"Downloaded {response.Response.Name} Rev {response.Response.Rev}");
                    Debug.WriteLine("------------------------------");
                    Debug.WriteLine(content);
                    Debug.WriteLine("------------------------------");

                    return content;
                }
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

                return null;
            }
        }

        public async Task<bool> Upload(string path, string content)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            try
            {
                var accessToken = Preferences.Get("dropbox_access_token", null);
                var refreshToken = Preferences.Get("dropbox_refresh_token", null);

                var config = new DropboxClientConfig("WordCross");

                using (var client = new DropboxClient(accessToken, refreshToken, ApiKey, ApiSecret, config))
                using (var stream = new MemoryStream(UTF8Encoding.UTF8.GetBytes(content)))
                {
                    var response = await client.Files.UploadAsync(path, WriteMode.Overwrite.Instance, body: stream);

                    Debug.WriteLine($"Uploaded Id {response.Id} Rev {response.Rev}");
                }

                return true;
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
        private async Task<string?> AcquireAccessToken(string[] scopeList, IncludeGrantedScopes includeGrantedScopes)
        {
            var state = Guid.NewGuid().ToString("N");

            var authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Code, ApiKey, RedirectUri, state: state, tokenAccessType: TokenAccessType.Offline, scopeList: scopeList, includeGrantedScopes: includeGrantedScopes);
            
            Debug.WriteLine("Waiting for credentials.");

            var listener = new HttpListener();

            listener.Prefixes.Add(LoopbackHost);

            try
            {           
                listener.Start();

                await Browser.Default.OpenAsync(authorizeUri, BrowserLaunchMode.SystemPreferred);

                /****** Handle OAuth redirect and send URL fragment to local server using JS. ******/

                var primaryContext = await listener.GetContextAsync();

                while (primaryContext.Request.Url.AbsolutePath != RedirectUri.AbsolutePath)
                {
                    primaryContext = await listener.GetContextAsync();
                }

                primaryContext.Response.ContentType = "text/html";

                // Respond with a page which runs JS and sends URL fragment as query string
                // to TokenRedirectUri.
                using (var file = await FileSystem.OpenAppPackageFileAsync("OAuthRedirect.html"))
                {
                    file.CopyTo(primaryContext.Response.OutputStream);
                    primaryContext.Response.OutputStream.Close();
                }

                /****** Handle redirect from JS and process OAuth response. ******/

                var secondaryContext = await listener.GetContextAsync();

                while (secondaryContext.Request.Url.AbsolutePath != SecondaryRedirectUri.AbsolutePath)
                {
                    secondaryContext = await listener.GetContextAsync();
                }

                var redirectUri = new Uri(secondaryContext.Request.QueryString["url_with_fragment"]);

                secondaryContext.Response.ContentType = "text/html";

                //アプリケーションに戻るよう表示
                using (var file = await FileSystem.OpenAppPackageFileAsync("OAuthSuccess.html"))
                {
                    file.CopyTo(secondaryContext.Response.OutputStream);
                    secondaryContext.Response.OutputStream.Close();
                }

                /****** Retrieve Tokens ******/

                Debug.WriteLine("Exchanging code for token");

                var tokenResult = await DropboxOAuth2Helper.ProcessCodeFlowAsync(redirectUri, ApiKey, ApiSecret, RedirectUri.ToString(), state);

                Debug.WriteLine("Finished Exchanging Code for Token");

                //トークンを保存
                Preferences.Default.Set("dropbox_access_token", tokenResult.AccessToken);

                if (!string.IsNullOrEmpty(tokenResult.RefreshToken))
                {
                    Preferences.Default.Set("dropbox_refresh_token", tokenResult.RefreshToken);
                }    

                return tokenResult.Uid;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error: {e.Message}");
                return null;
            }
            finally
            {
                listener.Stop();
                listener.Close();
            }
        }
    }
}

