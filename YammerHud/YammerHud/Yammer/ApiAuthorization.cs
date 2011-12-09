using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Hammock;
using Hammock.Authentication.OAuth;
using System.Windows.Threading;
using Microsoft.Phone.Controls;

namespace YammerHub.Yammer
{
    public class ApiAuthorization
    {

        OAuthToken requestToken;
        OAuthToken accessToken;
        WebBrowser browser;
        Action<OAuthToken> callback;

        public delegate void BeforeBrowsingBeginHandler();
        public event BeforeBrowsingBeginHandler BeforeBrowsingBegin;

        public delegate void AfterBrowsingCompleteHandler();
        public event AfterBrowsingCompleteHandler AfterBrowsingComplete;

        public ApiAuthorization(WebBrowser browser)
        {
            this.browser = browser;
            browser.Navigating += new EventHandler<NavigatingEventArgs>(browser_Navigating);
        }

        void browser_Navigating(object sender, NavigatingEventArgs e)
        {
            if (!e.Uri.AbsoluteUri.Contains("oauth_verifier"))
                return;

            if (AfterBrowsingComplete != null)
                AfterBrowsingComplete();

            e.Cancel = true;

            var arguments = e.Uri.AbsoluteUri.Split('?');
            if (arguments.Length < 1)
                return;

            GetAccessToken(arguments[1]);
        }

        private void GetAccessToken(string uri)
        {
            var requestVerifier = uri.GetQueryParameter("oauth_verifier");

            var credentials = new OAuthCredentials
            {
                Type = OAuthType.AccessToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = ApiSettings.ConsumerKey,
                ConsumerSecret = ApiSettings.ConsumerSecret,
                Token = requestToken.Key,
                TokenSecret = requestToken.Secret,
                Verifier = requestVerifier
            };

            var client = new RestClient
            {
                Authority = ApiSettings.Authority,
                Credentials = credentials,
                HasElevatedPermissions = true
            };

            var request = new RestRequest
            {
                Path = "oauth/access_token"
            };

            client.BeginRequest(request, new RestCallback(OAuthAccessTokenCallback));
        }

        private void OAuthAccessTokenCallback(RestRequest request, RestResponse response, object userState)
        {
            accessToken = new OAuthToken
            {
                Key = response.Content.GetQueryParameter("oauth_token"),
                Secret = response.Content.GetQueryParameter("oauth_token_secret")
            };

            if (callback != null)
                callback(accessToken);
        }


        public void BeginAuthorization(Action<OAuthToken> callback)
        {
            this.callback = callback;

            var credentials = new OAuthCredentials
            {
                Type = OAuthType.RequestToken,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = ApiSettings.ConsumerKey,
                ConsumerSecret = ApiSettings.ConsumerSecret,
                CallbackUrl = ApiSettings.CallbackUrl,
            };

            var client = new RestClient
            {
                Authority = ApiSettings.Authority,
                Credentials = credentials
            };

            var request = new RestRequest
            {
                Path = "oauth/request_token"
            };

            client.BeginRequest(request, OAuthRequestTokenCallback);
        }

        private void OAuthRequestTokenCallback(RestRequest request, RestResponse response, object userState)
        {
            requestToken = new OAuthToken
            {
                Key = response.Content.GetQueryParameter("oauth_token"),
                Secret = response.Content.GetQueryParameter("oauth_token_secret")
            };

            if (BeforeBrowsingBegin != null)
                BeforeBrowsingBegin();

            browser.Dispatcher.BeginInvoke(() =>
            {
                browser.Navigate(new Uri("https://www.yammer.com/oauth/authorize?oauth_token=" + requestToken.Key));
            });
        }

    }
}
