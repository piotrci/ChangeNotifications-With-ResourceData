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
    public class JwtTokenValidator
    {
        /// <summary>
        /// Uniquely identifies the Microsoft Graph change notification service that requests validation tokens.
        /// You must check if the tokens are originating from this appId to make sure this is actuall Microsoft Graph sending the tokens.
        /// </summary>
        const string expectedOriginAppId = "0bf30f3b-4a52-48df-9a82-234910c4a086";

        /// <summary>
        /// Validation tokens must be issued with the following issuer. Any other value means that the token was issued by something else than Azure AD.
        /// </summary>
        const string expectedTokenIssuer = "https://sts.windows.net/";

        public bool ValidateToken(string tokenBlob)
        {
            var audience = AuthSettings.applicationId;
            var jwtH = new JwtSecurityTokenHandler();
            var tokenIssuer = jwtH.ReadJwtToken(tokenBlob).Issuer;

            if (!tokenIssuer.StartsWith(expectedTokenIssuer, StringComparison.Ordinal))
            {
                throw new UnexpectedIssuerException($"Token issuer is unexpected: {tokenIssuer}. Should start with: {expectedTokenIssuer}");
            }
            

            IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"https://login.microsoftonline.com/common/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration openIdConfig = configurationManager.GetConfigurationAsync(CancellationToken.None).Result;

            var validationParams = new TokenValidationParameters
            {
                ValidIssuer = tokenIssuer,
                ValidAudiences = new[] { audience },
                IssuerSigningKeys = openIdConfig.SigningKeys,
                ValidateIssuerSigningKey = true
            };

            // check overall token validation, using the SDK
            SecurityToken token;
            try
            {
                jwtH.ValidateToken(tokenBlob, validationParams, out token);
            }
            catch (Exception ex)
            {
                throw new TokenValidationFailedException($"The validation token is not valid. See inner exception for details: {ex.GetType().Name}", ex);
            }

            // check that the origin application that obtained the token is the expected app represeting Microsoft Graph change notifications
            var jwtToken = (JwtSecurityToken)token;
            if (true)
            {
                
                //throw new IncorrectOriginAppException($"Token was requested by an unexpected app: {token.")
            }
            
            throw new NotImplementedException();
        }
    }
    public class TokenValidationFailedException : Exception
    {
        public TokenValidationFailedException(string message, Exception innerException) : base()
        {

        }
    }

    public class IncorrectOriginAppException : Exception
    {
        public IncorrectOriginAppException(string message, Exception innerException) : base()
        {

        }
    }

    public class UnexpectedIssuerException : Exception
    {
        public UnexpectedIssuerException(string message) : base()
        {

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