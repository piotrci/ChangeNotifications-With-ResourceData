using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CertificateUnitTests
{
    [TestClass]
    public class CertificateLoadingTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var certFiles = new[]
            {
                "CertFiles/KeyVaultCerFormat.cer",
                "CertFiles/PEMCertAsCer.cer",
                "CertFiles/PEMCertAsPFXPem.pem"
            };
            foreach (var certFile in certFiles)
            {  
                // simulates the payload we would receive thru request
                var j = LoadCertIntoJson(certFile);
                // simulates the code service-side: we take the string property and convert it into bytes
                var certText = j[certPropName].Value<string>();
                //byte[] certBytes = Convert.FromBase64String(certText);
                byte[] certBytes = Encoding.UTF8.GetBytes(certText);

                // simulates us loading the certificate from the request
                var x = new X509Certificate2();
                x.Import(certBytes);

                RSACryptoServiceProvider p = (RSACryptoServiceProvider)x.PublicKey.Key;
                // test encryption
                var toEncrypt = new byte[32];
                new Random().NextBytes(toEncrypt);
                var encryption = p.Encrypt(toEncrypt, true);
            }
            
            return;
        }
        const string certPropName = "cert";
        static private JObject LoadCertIntoJson(string filePath)
        {
            return MakeJsonWithCert(LoadCertAsText(filePath));
        }

        static private string LoadCertAsText(string filePath)
        {
            return File.ReadAllText(filePath);
        }
        static private JObject MakeJsonWithCert(string cert)
        {
            var j = new JObject();
            j.Add(certPropName, cert);
            return j;
        }
    }
}
