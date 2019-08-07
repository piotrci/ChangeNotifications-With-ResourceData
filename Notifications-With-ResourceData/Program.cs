using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using TokenValidation;

namespace DemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //goto subscription;
            var notifications = NotificationDownloader.GetNotificationsFromBlobs("https://testfunctionsfo97a4.blob.core.windows.net/notificationblobs?st=2019-08-06T22%3A40%3A12Z&se=2019-09-07T18%3A40%3A00Z&sp=rl&sv=2018-03-28&sr=c&sig=kz5ah8ziqBKn6oyX1FoNihfCSM1fVAc1qvvzwsvjA4c%3D",
                DateTime.Parse("2019-08-04"));

            var audiences = new[] { AuthSettings.applicationId };
            var validator = new JwtTokenValidator(audiences);
            validator.InitializeOpenIdConnectConfigurationAsync().Wait();

            foreach (var notifContent in notifications)
            {
                var p = new NotificationProcessor(notifContent);
                //p.ValidateAllTokens(validator);

                var results = p.DecryptAllNotifications().ToArray();

                
                

            }



            
            return;


            //KeyGeneration.GenerateKey();
            //return;
            // set up authentication based on the config specified in AuthSettings.cs (you should have a local git-ignoder AuthSettingsLocal.cs file where you initialize the values
            subscription:  var authProvider = AuthSettings.isUserAuthentication ? (MyAuthenticationProvider)new UserAuthenticationProvider() : (MyAuthenticationProvider)new AppOnlyAuthenticationProvider();
            GraphServiceClient client = GetAuthenticatedClient(authProvider);
            var token = authProvider.GetAccessTokenAsync().Result;

            var subManager = new SubscriptionManager(client, NotificationProcessingSettings.notificationUrl, NotificationProcessingSettings.lifecycleNotificationUrl);

            //var subs = subManager.GetAllSubscriptionsAsync().Result;
            subManager.DeleteAllSubscriptionsAsync().Wait();

            //var createdSub = subManager.CreateSubscriptionAsync("/users", "updated", "bobState").Result;
            var createdSub = subManager.CreateSubscriptionAsync("/teams/allMessages", "created,updated", TimeSpan.FromMinutes(58), "bobState", NotificationProcessingSettings.publicEncryptionKey, NotificationProcessingSettings.publicEncryptionKeyId, true).Result;

            return;
        }

        private static readonly string microsoftGraphV1 = @"https://graph.microsoft.com/v1.0";
        private static readonly string microsoftGraphCanary = @"https://canary.graph.microsoft.com//testencryptionnotification";

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
