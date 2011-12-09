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
using Hammock.Authentication.OAuth;
using Hammock;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YammerHub.Yammer
{
    public static class Api
    {

        private static RestClient CreateAuthorizedClient(OAuthToken accessToken)
        {
            var credentials = new OAuthCredentials
            {
                Type = OAuthType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = ApiSettings.ConsumerKey,
                ConsumerSecret = ApiSettings.ConsumerSecret,
                Token = accessToken.Key,
                TokenSecret = accessToken.Secret
            };

            return new RestClient
            {
                Authority = ApiSettings.Authority,
                Credentials = credentials,
                HasElevatedPermissions = true
            };
        }

        public static void BeginGetCompanyFeed(OAuthToken accessToken, Action<ApiResponse> callback)
        {
            CreateAuthorizedClient(accessToken).BeginRequest(new RestRequest { Path = "api/v1/messages.json" }, (req, response, o) =>
            {
                ApiResponse deserializedProduct = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                callback(deserializedProduct);
            });
        }

        public static void BeginGetMyFeed(OAuthToken accessToken, Action<ApiResponse> callback)
        {
            CreateAuthorizedClient(accessToken).BeginRequest(new RestRequest { Path = "api/v1/messages/following.json" }, (req, response, o) =>
            {
                ApiResponse deserializedProduct = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                callback(deserializedProduct);
            });
        }

        public static void BeginGetPrivateFeed(OAuthToken accessToken, Action<ApiResponse> callback)
        {
            CreateAuthorizedClient(accessToken).BeginRequest(new RestRequest { Path = "api/v1/messages/private.json" }, (req, response, o) =>
            {
                ApiResponse deserializedProduct = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                callback(deserializedProduct);
            });
        }

        public static void BeginGetSentFeed(OAuthToken accessToken, Action<ApiResponse> callback)
        {
            CreateAuthorizedClient(accessToken).BeginRequest(new RestRequest { Path = "api/v1/messages/sent.json" }, (req, response, o) =>
            {
                ApiResponse deserializedProduct = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                callback(deserializedProduct);
            });
        }

        public static void BeginGetReceivedFeed(OAuthToken accessToken, Action<ApiResponse> callback)
        {
            CreateAuthorizedClient(accessToken).BeginRequest(new RestRequest { Path = "api/v1/messages/received.json" }, (req, response, o) =>
            {
                ApiResponse deserializedProduct = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                callback(deserializedProduct);
            });
        }

        public static void BeginComposeMessage(OAuthToken accessToken, string body, Action<ApiResponse> callback)
        {
            var client = CreateAuthorizedClient(accessToken);

            var request = new RestRequest { Method = Hammock.Web.WebMethod.Post, Path = "api/v1/messages.json" };
            request.AddParameter("body", body);

            client.BeginRequest(request, (req, response, o) =>
            {
                ApiResponse deserializedProduct = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                callback(deserializedProduct);
            });
        }
    }
}
