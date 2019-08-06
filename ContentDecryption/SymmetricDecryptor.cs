using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ContentDecryption
{
    public static class SymmetricDecryptor
    {
        public static string Decrypt(byte[] encryptedPayload, byte[] aesKey)
        {
            using (var aesCrypto = new AesCryptoServiceProvider())
            {
                aesCrypto.Key = aesKey;
                aesCrypto.GenerateIV();
                using (var decryptor = aesCrypto.CreateDecryptor())
                {
                    using (MemoryStream msDecrypt = new MemoryStream(encryptedPayload))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
    }
}
