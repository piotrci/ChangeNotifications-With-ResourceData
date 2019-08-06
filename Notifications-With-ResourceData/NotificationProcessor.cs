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

        public string DecryptResourceData(JToken item)
        {
            const string keyIdProperty = "publicEncryptionKeyId";
            const string symmetricKeyProperty = "encryptedResourceDataKey";
            const string encryptedPayloadProperty = "encryptedResourceData";

            var resourceData = item["resourceData"];

            string keyId = resourceData[keyIdProperty]?.Value<string>() ?? throw new InvalidOperationException("Encryption key id does not exist in the notification payload");
            string encryptedSymmetricKey = resourceData[symmetricKeyProperty]?.Value<string>() ?? throw new InvalidOperationException("Symmetric key does not exist in the notification payload");
            string encryptedPayload = resourceData[encryptedPayloadProperty]?.Value<string>() ?? throw new InvalidOperationException("Encrypted payload ;sdoes not exist in the notification payload");

            string privateKey = DummyKeyStore.GetPrivateKey(keyId);

            RSACryptoServiceProvider crypto = new RSACryptoServiceProvider();
            crypto.FromXmlString(privateKey);

            byte[] payload = Convert.FromBase64String(encryptedSymmetricKey);

            var aesKey = crypto.Decrypt(payload, true);

            var symmCrypto = new AesCryptoServiceProvider();
            symmCrypto.Key = aesKey;
            symmCrypto.GenerateIV();

            var decryptor = symmCrypto.CreateDecryptor();
            string plainText;

            var cipherText = Convert.FromBase64String(encryptedPayload);

            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plainText = srDecrypt.ReadToEnd();
                    }
                }
            }

            throw new NotImplementedException();
        }
    }
}
