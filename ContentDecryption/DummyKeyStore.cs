using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentDecryption
{
    static public partial class DummyKeyStore
    {
        public static string GetPublicKeyLocal(string id)
        {
            return keys[id].PublicKey;
        }

        public static string GetPrivateKeyLocal(string id)
        {
            return keys[id].PrivateKey;
        }

        public static byte[] GetPublicKeyAzVault(string vaultSas, string keyName)
        {
            throw new NotImplementedException();
        }
        public static byte[] GetPrivateKeyAzVault(string vaultSas, string keyName)
        {
            throw new NotImplementedException();
        }



        private static readonly Dictionary<string, KeyPair> keys = null;

        private struct KeyPair
        {
            public string PublicKey { get;  set; }
            public string PrivateKey { get;  set; }
        }
    }
    
}
