using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ContentDecryption
{
    public static class KeyDecryptor
    {
        public static byte[] DecryptSymmetricKey(byte[] encryptedSymmetricKey, string privateKeyId)
        {
            string privateKey = DummyKeyStore.GetPrivateKey(privateKeyId);
            using (RSACryptoServiceProvider crypto = new RSACryptoServiceProvider())
            {
                crypto.FromXmlString(privateKey);
                return crypto.Decrypt(encryptedSymmetricKey, true); 
            }
        }
    }
}
