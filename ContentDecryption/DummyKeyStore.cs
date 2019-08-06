using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentDecryption
{
    static partial class DummyKeyStore
    {
        public static string GetPublicKey(string id)
        {
            return keys[id].PublicKey;
        }

        public static string GetPrivateKey(string id)
        {
            return keys[id].PrivateKey;
        }

        private static readonly Dictionary<string, KeyPair> keys = null;

        private struct KeyPair
        {
            public string PublicKey { get;  set; }
            public string PrivateKey { get;  set; }
        }
    }
    
}
