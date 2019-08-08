using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ContentDecryption
{
    public static class AsymmetricDecryptor
    {
        static AsymmetricDecryptor()
        {
            NameValueCollection cacheConfig = new NameValueCollection();
            cacheConfig.Add("cacheMemoryLimitMegabytes", "2");
            rsaProviderCache = new MemoryCache("rsaProviderCache", cacheConfig);
        }
        private static readonly MemoryCache rsaProviderCache;
        public static byte[] Decrypt(byte[] encryptedSymmetricKey, string privateKeyId)
        {
            if (encryptedSymmetricKey == null)
            {
                throw new ArgumentNullException(nameof(encryptedSymmetricKey));
            }

            if (privateKeyId == null)
            {
                throw new ArgumentNullException(nameof(privateKeyId));
            }

            var crypto = GetProvider(privateKeyId);
            return crypto.Decrypt(encryptedSymmetricKey, fOAEP: true);
        }
        private static RSACryptoServiceProvider GetProvider(string privateKeyId)
        {
            RSACryptoServiceProvider provider = rsaProviderCache[privateKeyId] as RSACryptoServiceProvider;
            if (provider == null)
            {
                var policy = new CacheItemPolicy
                {
                    RemovedCallback = DisposeRemovedProvider,
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };
                string privateKey = DummyKeyStore.GetPrivateKey(privateKeyId);
                RSACryptoServiceProvider newProvider = new RSACryptoServiceProvider();
                newProvider.FromXmlString(privateKey);

                provider = (RSACryptoServiceProvider)rsaProviderCache.AddOrGetExisting(privateKeyId, newProvider, policy) ?? newProvider;
            }
            return provider;
        }
        private static void DisposeRemovedProvider(CacheEntryRemovedArguments args)
        {
            var provider = args.CacheItem.Value as RSACryptoServiceProvider;
            if (provider != null)
            {
                provider.Dispose();
            }
        }
    }
}
