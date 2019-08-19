using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ContentDecryption;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Linq;

namespace CertificateUnitTests
{
    [TestClass]
    public class EncryptionTests
    {
        [TestMethod]
        public void TestAsymmetricDecryption()
        {
            var someBytes = new byte[32];
            new Random().NextBytes(someBytes);

            const string certId = "E96149FC-3B4F-4E0B-ACED-E715D29961FD";

            var key = Convert.FromBase64String(DummyKeyStore.GetPublicKeyLocal(certId));
            var cert = new X509Certificate2();
            cert.Import(key);

            byte[] encryptedBytes;
            using (var provider = (RSACryptoServiceProvider)cert.PublicKey.Key)
            {
                encryptedBytes = provider.Encrypt(someBytes, true);
            }

            var decryptedBytes = AsymmetricDecryptor.Decrypt(encryptedBytes, certId);
            Assert.IsTrue(someBytes.SequenceEqual(decryptedBytes));
        }
    }
}
