using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TokenValidation;

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

        public void ValidateAllTokens(JwtTokenValidator validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            validator.ValidateAllTokens(notif[tokens].Values<string>());
        }
    }
}
