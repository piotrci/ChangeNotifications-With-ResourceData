using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TokenValidation;
using ContentDecryption;
using Microsoft.Identity.Client;

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

        public void TestDecrypt()
        {
            var x = DecryptResourceData(notif[items].First);
        }

        public IEnumerable<NotificationItem> DecryptAllNotifications()
        {
            return notif[items].Select(i => new NotificationItem(i, DecryptResourceData(i)));
        }


        public JToken DecryptResourceData(JToken item)
        {
            const string encryptedContentProperty = "encryptedContent";
            const string certificateIdProperty = "encryptionCertificateId";
            const string symmetricKeyProperty = "dataKey";
            const string encryptedPayloadProperty = "data";
            const string signatureProperty = "dataSignature";

            var encryptedContent = item[encryptedContentProperty];

            string certificateId = encryptedContent[certificateIdProperty]?.Value<string>() ?? throw new InvalidOperationException("Encryption key id does not exist in the notification payload");
            string encryptedSymmetricKey = encryptedContent[symmetricKeyProperty]?.Value<string>() ?? throw new InvalidOperationException("Symmetric key does not exist in the notification payload");
            string encryptedPayload = encryptedContent[encryptedPayloadProperty]?.Value<string>() ?? throw new InvalidOperationException("Encrypted payload ;sdoes not exist in the notification payload");
            string hashMac = encryptedContent[signatureProperty]?.Value<string>() ?? throw new InvalidOperationException("Encrypted signature does not exist in the notification payload");

            var payloadBytes = Convert.FromBase64String(encryptedPayload);
            var signatureBytes = Convert.FromBase64String(hashMac);

            // descrypt the symetric key
            var symmetricKey = AsymmetricDecryptor.Decrypt(Convert.FromBase64String(encryptedSymmetricKey), certificateId);

            // verify signature using the symmetric key
            SymmetricDecryptor.VerifyHMACSignature(payloadBytes, symmetricKey, signatureBytes);
            
            // decrypt payload using symmetric key
            string plainText = SymmetricDecryptor.Decrypt(payloadBytes, symmetricKey);

            return JToken.Parse(plainText);
        }
    }
    public struct NotificationItem
    {
        public NotificationItem(JToken originalNotification, JToken decryptedContent)
        {
            OriginalNotification = originalNotification;
            DecryptedContent = decryptedContent;
        }

        public JToken OriginalNotification { get; }
        public JToken DecryptedContent { get; }
    }
}
