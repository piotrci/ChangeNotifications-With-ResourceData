using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using TokenValidation;
using ContentDecryption;
using System.Threading;

namespace DemoApp
{
    class Program
    {
        static void Main(string[] args)
        {
            const string queueSasKey = "https://testfunctionsfo97a4.queue.core.windows.net/notifqueue?st=2019-09-05T23%3A00%3A14Z&se=2019-10-06T20%3A00%3A00Z&sp=rup&sv=2018-03-28&sig=VgevMRmMB0miZbIQzpOgteyrIlLbwGKfsO48dJ%2F2WtQ%3D";
            const string blobSasKey = "https://testfunctionsfo97a4.blob.core.windows.net/notificationblobs?st=2019-08-06T22%3A40%3A12Z&se=2019-09-07T18%3A40%3A00Z&sp=rl&sv=2018-03-28&sr=c&sig=kz5ah8ziqBKn6oyX1FoNihfCSM1fVAc1qvvzwsvjA4c%3D";

            var authProvider = AuthSettings.isUserAuthentication ? (MyAuthenticationProvider)new UserAuthenticationProvider() : (MyAuthenticationProvider)new AppOnlyAuthenticationProvider();
            GraphServiceClient client = GetAuthenticatedClient(authProvider);
            var token = authProvider.GetAccessTokenAsync().Result;

            var subManager = new SubscriptionManager(client, NotificationProcessingSettings.notificationUrl, NotificationProcessingSettings.lifecycleNotificationUrl);

            //var subs = subManager.GetAllSubscriptionsAsync().Result;
            subManager.DeleteAllSubscriptionsAsync().Wait();

            //var createdSub = subManager.CreateSubscriptionAsync("/users", "updated", "bobState").Result;
            var createdSub = subManager.CreateSubscriptionAsync("/teams/allMessages", "created,updated", TimeSpan.FromMinutes(58), "bobState", DummyKeyStore.GetPublicKeyLocal(NotificationProcessingSettings.encryptionCertificateId), NotificationProcessingSettings.encryptionCertificateId, true).Result;

            var messenger = new MessageManager(microsoftGraphCanary, "95432da5-e897-4fd4-8141-3df339ca1141", "19:35150d8a0302476ba9f516873f6b06d6@thread.skype");
            var ct = new CancellationToken();
            var messengerTask = messenger.StartAsync(ct);

            Console.WriteLine("Subscription created. Waiting for notifications.");
            var notifications = NotificationDownloader.LoopOverNotificationsFromQueue(queueSasKey, messengerTask);
            
            //var notifications = NotificationDownloader.GetNotificationsFromBlobs(blobSasKey, DateTime.Parse("2019-08-04"));

            var audiences = new[] { AuthSettings.applicationId };
            var validator = new JwtTokenValidator(audiences);
            validator.InitializeOpenIdConnectConfigurationAsync().Wait();

            foreach (var notifContent in notifications)
            {
                var p = new NotificationProcessor(notifContent);
                p.ValidateAllTokens(validator);

                // renew any subscriptions that require re-authorization
                foreach (var subId in p.GetSubscriptionsToReauthorize())
                {
                    subManager.RenewSubscriptionAsync(subId, TimeSpan.FromMinutes(58)).Wait();
                }

                var results = p.DecryptAllNotifications().ToArray();
                // print portions of the content to console, just for fun
                foreach (var notif in results)
                {
                    PrintContentToConsole(notif);
                }
            }
            return;
        }

        private static void PrintContentToConsole(NotificationItem notif)
        {
            var content = notif.DecryptedContent["body"]?.Value<string>("content") ?? "<no message>";
            Console.WriteLine(content);
        }

        private static readonly string microsoftGraphV1 = @"https://graph.microsoft.com/v1.0";
        private static readonly string microsoftGraphCanary = @"https://canary.graph.microsoft.com/testencryptionnotification3";
        //private static readonly string microsoftGraphCanary = @"https://canary.graph.microsoft.com/beta";

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
