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
    public class PayloadDecryptor : IDisposable
    {
        public PayloadDecryptor(byte[] aesKey)
        {
            using (var aesCrypto = new AesCryptoServiceProvider())
            {
                aesCrypto.Key = aesKey;
                aesCrypto.GenerateIV();
                this.decryptor = aesCrypto.CreateDecryptor();
            }
        }
        private readonly ICryptoTransform decryptor;

        public string Decrypt(byte[] encryptedPayload)
        {
            using (MemoryStream msDecrypt = new MemoryStream(encryptedPayload))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, this.decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    this.decryptor.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PayloadDecryptor()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
