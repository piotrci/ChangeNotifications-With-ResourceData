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
                aesCrypto.Padding = PaddingMode.PKCS7;
                aesCrypto.Mode = CipherMode.CBC;

                var vectorSize = aesCrypto.BlockSize / 8;
                byte[] iv = new byte[vectorSize];
                Array.Copy(aesKey, iv, vectorSize);
                aesCrypto.IV = iv;

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
        public static bool VerifyHMACSignature(byte[] encryptedPayload, byte[] key, byte[] expectedSignature)
        {
            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                byte[] actualSignature = hmac.ComputeHash(encryptedPayload);
                return actualSignature.SequenceEqual(expectedSignature);
            }
        }
    }
}
