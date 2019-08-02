using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace DemoApp
{
    class JwtTokenValidator
    {
        public bool ValidateToken(string tokenBlob)
        {
            var audience = AuthSettings.applicationId;
            var jwtH = new JwtSecurityTokenHandler();
            var readToken = jwtH.ReadJwtToken(tokenBlob);
            

            IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"https://login.microsoftonline.com/common/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).Result;

            var validationParams = new TokenValidationParameters
            {
                ValidIssuer = readToken.Issuer,
                ValidAudiences = new[] { audience },
                IssuerSigningKeys = openIdConfig.SigningKeys
            };
            
            jwtH.ValidateToken(tokenBlob, validationParams, out SecurityToken token);
            throw new NotImplementedException();
        }
    }
}

//{
//  "typ": "JWT",
//  "alg": "RS256",
//  "x5t": "u4OfNFPHwEBosHjtrauObV84LnY",
//  "kid": "u4OfNFPHwEBosHjtrauObV84LnY"
//}.{
//  "aud": "8e460676-ae3f-4b1e-8790-ee0fb5d6148f",
//  "iss": "https://sts.windows.net/2bcd9dfb-6a66-4236-98f9-e8fe4a675323/",
//  "iat": 1564766994,
//  "nbf": 1564766994,
//  "exp": 1564796094,
//  "aio": "42FgYDCRfq/VUXzsW1lC6ZTS1LlxAA==",
//  "appid": "0bf30f3b-4a52-48df-9a82-234910c4a086",
//  "appidacr": "2",
//  "idp": "https://sts.windows.net/2bcd9dfb-6a66-4236-98f9-e8fe4a675323/",
//  "tid": "2bcd9dfb-6a66-4236-98f9-e8fe4a675323",
//  "uti": "AGZ4GNSNVkW11XfxzkUjAA",
//  "ver": "1.0"
//}.[Signature] 