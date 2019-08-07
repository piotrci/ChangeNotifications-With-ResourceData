using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace TokenValidation
{
    public class JwtTokenValidator
    {
        public JwtTokenValidator(IEnumerable<string> audiences)
        {
            this.audiences = audiences ?? throw new ArgumentNullException(nameof(audiences));
        }
        /// <summary>
        /// Initializes the configuration for Azure AD, by connecting to the common configuration endpoint
        /// </summary>
        public async Task InitializeOpenIdConnectConfigurationAsync()
        {
            OpenIdConnectConfiguration openIdConfig = await GetCommonOpenIdConfig();
            this.InitializeOpenIdConnectConfiguration(openIdConfig);
        }
        public void InitializeOpenIdConnectConfiguration(OpenIdConnectConfiguration openIdConfig)
        {
            this.signingKeys = openIdConfig.SigningKeys;
            this.issuerTemplate = new Uri(openIdConfig.Issuer);
            this.configInitialized = true;
        }
        private static async Task<OpenIdConnectConfiguration> GetCommonOpenIdConfig()
        {
            IConfigurationManager<OpenIdConnectConfiguration> configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>($"https://login.microsoftonline.com/common/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration openIdConfig = await configurationManager.GetConfigurationAsync(CancellationToken.None);
            return openIdConfig;
        }
        private readonly IEnumerable<string> audiences;
        private ICollection<SecurityKey> signingKeys;
        private Uri issuerTemplate;
        private bool configInitialized;


        /// <summary>
        /// Uniquely identifies the Microsoft Graph change notification service that requests validation tokens.
        /// You must check if the tokens are originating from this appId to make sure this is actuall Microsoft Graph sending the tokens.
        /// </summary>
        const string expectedOriginAppId = "0bf30f3b-4a52-48df-9a82-234910c4a086";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenBlob"></param>
        /// <exception cref="TokenValidationFailedException"></exception>
        /// <exception cref="UnexpectedIssuerException"></exception>
        /// <exception cref="IncorrectOriginAppException"></exception>
        public void ValidateToken(string tokenBlob)
        {
            if (!this.configInitialized)
            {
                throw new InvalidOperationException($"You must initalize the Open ID Connect config before validating any tokens. Call {nameof(this.InitializeOpenIdConnectConfiguration)} or {nameof(this.InitializeOpenIdConnectConfiguration)} first.");
            }
            var jwtH = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidAudiences = this.audiences,
                IssuerSigningKeys = this.signingKeys,
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false
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
            if (!jwtToken.Payload.TryGetValue("appid", out object appId) || !expectedOriginAppId.Equals((string)appId, StringComparison.Ordinal))
            {
                throw new IncorrectOriginAppException($"Token was requested by an unexpected app: {appId}. Expected app for Microsoft Graph change notifications is {expectedOriginAppId}.");
            }
        }
        /// <summary>
        /// Validates all tokens in the collection. Throws an exception when the first invalid token is found.
        /// </summary>
        /// <param name="validator"></param>
        public void ValidateAllTokens(IEnumerable<string> tokens)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            int i = 0;
            foreach (var token in tokens)
            {
                ++i;
                try
                {
                    this.ValidateToken(token);
                }
                catch (Exception ex) when (ex is TokenValidationFailedException || ex is UnexpectedIssuerException || ex is IncorrectOriginAppException)
                {
                    throw new InvalidTokenInCollectionException($"Validation token no {i} has failed validation. See inner exception for details.", token, ex);
                }
            }
        }



    }
    public class InvalidTokenInCollectionException : Exception
    {
        public InvalidTokenInCollectionException(string message, string token, Exception innerException) : base()
        {
            base.Data.Add("invalidToken", token);
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
        public IncorrectOriginAppException(string message) : base()
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