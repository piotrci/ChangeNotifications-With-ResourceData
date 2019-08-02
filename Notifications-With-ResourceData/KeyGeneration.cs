using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp
{
    static class KeyGeneration
    {
        static public void GenerateKey()
        {
            var cert = @"c:\t\keys\publickey.cer";
            cert = @"c:\t\MySelfSignedCert_ef14e51c53b24c23bf66c0ece6a033db.cer";
            cert = @"c:\t\peterskeyvault-MySelfSignedCert-20190801.pfx";

            var x = new X509Certificate2();
            x.Import(cert);
            RSACryptoServiceProvider p = (RSACryptoServiceProvider)x.PublicKey.Key;
            var publichKeyXml = p.ToXmlString(false);
            return;
        }
    }
}
