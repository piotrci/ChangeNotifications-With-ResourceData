using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DemoApp
{
    class NotificationProcessor
    {
        const string items = "value";
        const string tokens = "validationTokens";
        public NotificationProcessor(string notificationJson)
        {
            notif = JObject.Parse(notificationJson);
            if (notif[items] is JArray == false)
            {
                throw new ArgumentException($"Notification's '{items}' must be an array.");
            }
            if (notif[tokens] is JArray == false)
            {
                throw new ArgumentException($"Notification's '{tokens}' must be an array.");
            }
        }
        private readonly JObject notif;

        public void ValidateAllTokens()
        {
            int i = 0;
            var validator = new JwtTokenValidator();
            foreach (var token in notif[tokens])
            {
                ++i;
                try
                {
                    validator.ValidateToken(token.Value<string>());
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Validation token no {i} has failed validation. See inner exception for details.", ex);
                }
            }
        }
    }
}
