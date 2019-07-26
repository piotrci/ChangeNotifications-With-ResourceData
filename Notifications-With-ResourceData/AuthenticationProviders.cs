﻿using Microsoft.Graph;
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

        protected abstract string InitializeAppAndGetFirstToken();
        protected abstract Task<string> GetTokenSilentlyAsync();

        async public Task<string> GetAccessTokenAsync()
        {
            if (!this.isInitialized)
            {
                lock (this.initializationLock)
                {
                    if (!this.isInitialized)
                    {
                        this.isInitialized = true;
                        return this.InitializeAppAndGetFirstToken();
                    }
                }
            }
            return await this.GetTokenSilentlyAsync();
        }
    }
    class UserAuthenticationProvider : MyAuthenticationProvider
    {
        private IPublicClientApplication app;
        private IAccount account;
        async protected override Task<string> GetTokenSilentlyAsync()
        {
            return (await this.app.AcquireTokenSilent(AuthSettings.scopes, this.account).ExecuteAsync()).AccessToken;
        }

        protected override string InitializeAppAndGetFirstToken()
        {
            this.app = PublicClientApplicationBuilder.Create(AuthSettings.applicationId).Build();
            var authResult = this.app.AcquireTokenInteractive(AuthSettings.scopes).ExecuteAsync().Result;
            this.account = authResult.Account;
            return authResult.AccessToken;
        }
    }
    class AppOnlyAuthenticationProvider : MyAuthenticationProvider
    {
        private readonly static string[] scopes = new[] { "https://graph.microsoft.com/.default" };
        private IConfidentialClientApplication app;

        async protected override Task<string> GetTokenSilentlyAsync()
        {
            return (await this.app.AcquireTokenForClient(scopes).ExecuteAsync()).AccessToken;
        }

        protected override string InitializeAppAndGetFirstToken()
        {
            this.app = ConfidentialClientApplicationBuilder.Create(AuthSettings.applicationId).WithAuthority($"https://login.microsoftonline.com/{AuthSettings.tenantId}").WithClientSecret(AuthSettings.secretClientCredentials).Build();
            //this.app = new ConfidentialClientApplication(AuthSettings.applicationId, $"https://login.microsoftonline.com/{AuthSettings.tenantId}", "https://microsoft.com", AuthSettings.secretClientCredentials, null, new TokenCache());
            return GetTokenSilentlyAsync().Result;
        }
    }
}
