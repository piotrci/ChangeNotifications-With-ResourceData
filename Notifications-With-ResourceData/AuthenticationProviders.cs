using Microsoft.Graph;
using Microsoft.Identity.Client;
using DemoApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DemoApp
{
    abstract class MyAuthenticationProvider
    {
        private bool isInitialized = false;
        private readonly object initializationLock = new object();

        protected abstract string InitializeAppAndGetFirstToken(string[] scopes);
        protected abstract Task<string> GetTokenSilentlyAsync(string[] scopes);

        private readonly string[] scopes;

        async public Task<string> GetAccessTokenAsync()
        {
            return await this.GetAccessTokenAsync(AuthSettings.scopes);
        }
        async public Task<string> GetAccessTokenAsync(string[] scopes)
        {
            if (!this.isInitialized)
            {
                lock (this.initializationLock)
                {
                    if (!this.isInitialized)
                    {
                        this.isInitialized = true;
                        return this.InitializeAppAndGetFirstToken(scopes);
                    }
                }
            }
            return await this.GetTokenSilentlyAsync(scopes);
        }
    }
    class UserAuthenticationProvider : MyAuthenticationProvider
    {
        private IPublicClientApplication app;
        private IAccount account;
        async protected override Task<string> GetTokenSilentlyAsync(string[] scopes)
        {
            return (await this.app.AcquireTokenSilent(scopes, this.account).ExecuteAsync()).AccessToken;
        }

        protected override string InitializeAppAndGetFirstToken(string[] scopes)
        {
            this.app = PublicClientApplicationBuilder.Create(AuthSettings.applicationId).WithRedirectUri(AuthSettings.redirectUri).Build();
            var authResult = this.app.AcquireTokenInteractive(scopes).ExecuteAsync().Result;
            this.account = authResult.Account;
            return authResult.AccessToken;
        }
    }
    class AppOnlyAuthenticationProvider : MyAuthenticationProvider
    {
        private readonly static string graphCanary = "https://canary.graph.microsoft.com";
        private readonly static string graphProd = "https://graph.microsoft.com";
        private readonly static string[] scopes = new[] { $"{graphCanary}/.default" };
        private IConfidentialClientApplication app;

        async protected override Task<string> GetTokenSilentlyAsync(string[] scopes)
        {
            return (await this.app.AcquireTokenForClient(AppOnlyAuthenticationProvider.scopes).ExecuteAsync()).AccessToken;
        }

        protected override string InitializeAppAndGetFirstToken(string[] scopes)
        {
            this.app = ConfidentialClientApplicationBuilder.Create(AuthSettings.applicationId).WithAuthority($"https://login.microsoftonline.com/{AuthSettings.tenantId}").WithClientSecret(AuthSettings.secretClientCredentials).Build();
            //this.app = new ConfidentialClientApplication(AuthSettings.applicationId, $"https://login.microsoftonline.com/{AuthSettings.tenantId}", "https://microsoft.com", AuthSettings.secretClientCredentials, null, new TokenCache());
            return GetTokenSilentlyAsync(scopes).Result;
        }
    }
}
