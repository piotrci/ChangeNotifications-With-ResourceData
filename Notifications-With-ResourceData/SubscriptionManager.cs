using Microsoft.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp
{
    class SubscriptionManager
    {
        private readonly GraphServiceClient client;
        private readonly string notificationUrl;
        private readonly string lifeCycleNotificationUrl;

        public SubscriptionManager(GraphServiceClient client, string notificationUrl, string lifeCycleNotificationUrl)
        {
            if (string.IsNullOrEmpty(notificationUrl))
            {
                throw new ArgumentException("message", nameof(notificationUrl));
            }

            if (string.IsNullOrEmpty(lifeCycleNotificationUrl))
            {
                throw new ArgumentException("message", nameof(lifeCycleNotificationUrl));
            }

            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.notificationUrl = notificationUrl;
            this.lifeCycleNotificationUrl = lifeCycleNotificationUrl;
        }
        async public Task<Subscription> CreateSubscriptionAsync(string graphResource, string changeType, TimeSpan duration, string clientState, string encryptionKey, string encryptionKeyId, bool includeResourceData = false)
        {
            var sub = new Subscription();
            sub.AdditionalData = new Dictionary<string, object>();
            sub.AdditionalData.Add("lifecycleNotificationUrl", this.lifeCycleNotificationUrl);
            if (includeResourceData)
            {
                sub.AdditionalData.Add("includeResourceData", includeResourceData);
                if (String.IsNullOrEmpty(encryptionKey))
                {
                    throw new ArgumentNullException(nameof(encryptionKey), $"{nameof(encryptionKey)} must be set when {includeResourceData} is set to true.");
                }
                if (String.IsNullOrEmpty(encryptionKeyId))
                {
                    throw new ArgumentNullException(nameof(encryptionKeyId), $"{nameof(encryptionKeyId)} must be set when {includeResourceData} is set to true.");
                }
                sub.AdditionalData.Add("encryptionCertificate", encryptionKey);
                sub.AdditionalData.Add("encryptionCertificateId", encryptionKeyId);
            }
            sub.ChangeType = changeType;
            sub.NotificationUrl = this.notificationUrl;
            sub.Resource = graphResource;
            sub.ExpirationDateTime = DateTime.UtcNow + duration;
            sub.ClientState = clientState;
            return await client.Subscriptions.Request().AddAsync(sub);
        }
        async public Task<Subscription> CreateSubscriptionAsync(string graphResource, string changeType, string clientState)
        {
            return await CreateSubscriptionAsync(graphResource, changeType, TimeSpan.FromDays(2), clientState, null, null);
        }

        async public Task<Subscription> RenewSubscriptionAsync(string subscriptionId, TimeSpan duration)
        {
            var sub = new Subscription();
            sub.ExpirationDateTime = DateTime.UtcNow + duration;
            return await client.Subscriptions[subscriptionId].Request().UpdateAsync(sub);
        }

        async public Task DeleteAllSubscriptionsAsync()
        {
            var subs = await GetAllSubscriptionsAsync();
            foreach (var sub in subs)
            {
                await client.Subscriptions[sub.Id].Request().DeleteAsync();
            }
        }

        async public Task<IEnumerable<Subscription>> GetAllSubscriptionsAsync()
        {
            var currentPage = (await client.Subscriptions.Request().GetAsync());
            var items = currentPage.ToList();

            var nextRequest = currentPage.NextPageRequest;
            while (nextRequest != null)
            {
                currentPage = await nextRequest.GetAsync();
                items.AddRange(currentPage);
                nextRequest = currentPage.NextPageRequest;
            }
            return items;
        }
    }
}
