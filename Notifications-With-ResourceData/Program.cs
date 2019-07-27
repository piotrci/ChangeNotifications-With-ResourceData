using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;

namespace DemoApp
{
    class Program
    {
        static void Main(string[] args)
        {

            // set up authentication based on the config specified in AuthSettings.cs (you should have a local git-ignoder AuthSettingsLocal.cs file where you initialize the values
            var authProvider = AuthSettings.isUserAuthentication ? (MyAuthenticationProvider)new UserAuthenticationProvider() : (MyAuthenticationProvider)new AppOnlyAuthenticationProvider();
            GraphServiceClient client = GetAuthenticatedClient(authProvider);
            var token = authProvider.GetAccessTokenAsync().Result;

            var subManager = new SubscriptionManager(client, NotificationProcessingSettings.notificationUrl, NotificationProcessingSettings.lifecycleNotificationUrl);

            var subs = subManager.GetAllSubscriptionsAsync().Result;

            //subManager.DeleteAllSubscriptionsAsync().Wait();

            var createdSub = subManager.CreateSubscriptionAsync("/users", "updated", "bobState").Result;

            

            return;
        }

        private static readonly string microsoftGraphV1 = @"https://graph.microsoft.com/v1.0";
        private static readonly string microsoftGraphCanary = @"https://canary.graph.microsoft.com/testencryption";

        private static GraphServiceClient GetAuthenticatedClient(MyAuthenticationProvider provider)
        {
            GraphServiceClient client;

            client = new GraphServiceClient(
                microsoftGraphCanary,
                new DelegateAuthenticationProvider(
                    async (requestMessage) =>
                    {
                        var token = await provider.GetAccessTokenAsync();
                        requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
                    }));
            return client;
        }
    }
}
