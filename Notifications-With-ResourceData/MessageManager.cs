using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class MessageManager
    {
        private readonly HttpClient httpClient;
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly string graphEndpoint;
        private readonly string teamId;
        private readonly string channelId;
        private readonly MyAuthenticationProvider authProvider;

        public MessageManager(string graphEndpoint, string teamId, string channelId)
        {
            this.httpClient = new HttpClient();
            this.graphEndpoint = graphEndpoint;
            this.teamId = teamId;
            this.channelId = channelId;

            this.authProvider = (MyAuthenticationProvider)new UserAuthenticationProvider();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                string[] scopes = new string[] { "Group.ReadWrite.All" };
                while (true)
                {
                    if (this.tokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    string url = $"{this.graphEndpoint}/teams/{this.teamId}/channels/{this.channelId}/messages";

                    HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url);

                    var payload = new
                    {
                        body = new
                        {
                            content = "HelloWorld " + DateTimeOffset.Now.ToString(),
                        },
                    };

                    httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(payload));

                    var token = await this.authProvider.GetAccessTokenAsync(scopes);
                    httpRequestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse(token);
                    httpRequestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                    HttpResponseMessage responseMessage = await this.httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

                    string requestId = responseMessage.Headers.GetValues("request-id").FirstOrDefault();
                    string clientRequestId = responseMessage.Headers.GetValues("client-request-id").FirstOrDefault();

                    await Task.Delay(TimeSpan.FromSeconds(15));
                }
            });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            tokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}
