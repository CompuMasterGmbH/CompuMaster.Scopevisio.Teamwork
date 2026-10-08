using CenterDevice.Rest.Clients.OAuth;
using CompuMaster.Scopevisio.CenterDeviceApi;
using CompuMaster.Scopevisio.OpenApi;
using CompuMaster.Scopevisio.OpenApi.Client;
using CompuMaster.Scopevisio.OpenApi.Model;
using CompuMaster.Scopevisio.Teamwork;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.Teamwork.AsyncTests
{
    [TestFixture]
    public class TeamworkAuthorizationTests
    {
        [Test]
        public async Task AuthorizationFactoryUsesTheSharedTransportAndInstallsTheFreshIdentity()
        {
            var handler = new FakeHandler((request, token) => Task.FromResult(request.Method == HttpMethod.Get ? AccountResponse() : TokenHttpResponse()));
            using (var transport = CenterDevice.Rest.Clients.CenterDeviceHttpTransport.CreateHttpClient(handler))
            {
                var configuration = new Configuration { HttpClient = transport, BasePath = "https://" + Guid.NewGuid().ToString("N") + ".invalid/", ClientNumber = "fixture-customer", Username = "fixture-login", Password = "fixture-password" };
                var io = await TeamworkIOClient.AuthorizeAsync(configuration);
                Assert.That(handler.Calls, Is.EqualTo(2));
                Assert.That(handler.TokenCalls, Is.EqualTo(1));
                Assert.That(io.TeamworkRestClient.OpenscopeClient.Config, Is.SameAs(configuration));
                Assert.That(io.TeamworkRestClient.OpenscopeClient.Token.AccessToken, Is.EqualTo("fresh-token"));
                Assert.That(io.CurrentAuthenticationContextUserID, Is.EqualTo("fixture-user-id"));
                Assert.That(handler.LastTokenBody, Does.Contain("grant_type=password"));
            }
        }

        [Test]
        public async Task FactoriesShareAccountLookupAndPreserveTokenAndLoginIdentities()
        {
            var handler = new FakeHandler((request, token) => Task.FromResult(AccountResponse()));
            using (var transport = new HttpClient(handler))
            {
                var client = CreateClient(transport);
                var rest = await TeamworkRestClient.CreateAsync(client);
                var io = await TeamworkIOClient.CreateAsync(client);
                var provider = new TeamworkOAuthInfoProvider(client);
                var info = await provider.GetOAuthInfoAsync("fixture-login");
                Assert.That(handler.Calls, Is.EqualTo(1));
                Assert.That(rest.OpenscopeClient, Is.SameAs(client));
                Assert.That(io.TeamworkRestClient.OpenscopeClient, Is.SameAs(client));
                Assert.That(io.CurrentAuthenticationContextUserID, Is.EqualTo("fixture-user-id"));
                Assert.That(info.UserId, Is.EqualTo("fixture-user-id"));
                Assert.That(info.Email, Is.EqualTo("fixture-login"));
                Assert.That(info.TenantId, Is.EqualTo("fixture-tenant"));
                Assert.That(info.access_token, Is.EqualTo("old-token"));
            }
        }

        [Test]
        public async Task CancellationReachesTheActiveAccountRequest()
        {
            var entered = Signal();
            var handler = new FakeHandler(async (request, token) =>
            {
                entered.SetResult(true);
                await Task.Delay(Timeout.Infinite, token);
                return AccountResponse();
            });
            using (var transport = new HttpClient(handler))
            using (var cancellation = new CancellationTokenSource())
            {
                var operation = TeamworkRestClient.CreateAsync(CreateClient(transport), cancellation.Token);
                await entered.Task;
                cancellation.Cancel();
                Assert.CatchAsync<OperationCanceledException>((Func<Task>)(async () => { await operation; }));
            }
        }

        [Test]
        public async Task CanceledLookupWaiterDoesNotCancelTheOwnerOrSendAnotherRequest()
        {
            var entered = Signal();
            var release = Signal();
            var handler = new FakeHandler(async (request, token) =>
            {
                entered.SetResult(true);
                await release.Task;
                return AccountResponse();
            });
            using (var transport = new HttpClient(handler))
            using (var cancellation = new CancellationTokenSource())
            {
                var client = CreateClient(transport);
                var owner = TeamworkRestClient.CreateAsync(client);
                await entered.Task;
                var waiter = TeamworkRestClient.CreateAsync(client, cancellation.Token);
                cancellation.Cancel();
                Assert.CatchAsync<OperationCanceledException>((Func<Task>)(async () => { await waiter; }));
                Assert.That(owner.IsCompleted, Is.False);
                release.SetResult(true);
                await owner;
                Assert.That(handler.Calls, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task ConcurrentRefreshesOfTheSameRejectedTokenSendOnlyOneTokenRequest()
        {
            var entered = Signal();
            var release = Signal();
            var handler = new FakeHandler(async (request, token) =>
            {
                if (request.Method == HttpMethod.Get) return AccountResponse();
                entered.SetResult(true);
                await release.Task;
                return TokenHttpResponse();
            });
            using (var transport = new HttpClient(handler))
            {
                var client = CreateClient(transport);
                var first = new TeamworkClientErrorHandler("fixture-login", new TeamworkOAuthInfoProvider(client));
                var second = new TeamworkClientErrorHandler("fixture-login", new TeamworkOAuthInfoProvider(client));
                var rejected = new OAuthInfo { access_token = "old-token" };
                var owner = first.RefreshTokenAsync(rejected);
                await entered.Task;
                var follower = second.RefreshTokenAsync(rejected);
                release.SetResult(true);
                var values = await Task.WhenAll(owner, follower);
                Assert.That(handler.TokenCalls, Is.EqualTo(1));
                Assert.That(handler.Calls, Is.EqualTo(2));
                Assert.That(handler.LastTokenBody, Does.Contain("refresh_token=old-refresh"));
                Assert.That(values[0].access_token, Is.EqualTo("fresh-token"));
                Assert.That(values[1].access_token, Is.EqualTo("fresh-token"));
                Assert.That(client.Config.AccessToken, Is.EqualTo("fresh-token"));
            }
        }

        [Test]
        public async Task FailedAccountInitializationIsNotCached()
        {
            var count = 0;
            var handler = new FakeHandler((request, token) => Task.FromResult(
                ++count == 1 ? JsonResponse("{\"user\":null}") : AccountResponse()));
            using (var transport = new HttpClient(handler))
            {
                var client = CreateClient(transport);
                Assert.ThrowsAsync<InvalidOperationException>((Func<Task>)(async () => { await TeamworkRestClient.CreateAsync(client); }));
                var result = await TeamworkRestClient.CreateAsync(client);
                Assert.That(result.ApplicationContext.User.Login, Is.EqualTo("fixture-login"));
                Assert.That(handler.Calls, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task CanceledRefreshPreservesTokenAndReleasesTheRefreshGateForRetry()
        {
            var entered = Signal();
            var handler = new FakeHandler(async (request, token) =>
            {
                if (request.Method == HttpMethod.Get) return AccountResponse();
                if (entered.TrySetResult(true)) await Task.Delay(Timeout.Infinite, token);
                return TokenHttpResponse();
            });
            using (var transport = new HttpClient(handler))
            using (var cancellation = new CancellationTokenSource())
            {
                var client = CreateClient(transport);
                var error = new TeamworkClientErrorHandler("fixture-login", new TeamworkOAuthInfoProvider(client));
                var rejected = new OAuthInfo { access_token = "old-token" };
                var operation = error.RefreshTokenAsync(rejected, cancellation.Token);
                await entered.Task;
                cancellation.Cancel();
                Assert.CatchAsync<OperationCanceledException>((Func<Task>)(async () => { await operation; }));
                Assert.That(client.Token.AccessToken, Is.EqualTo("old-token"));
                var refreshed = await error.RefreshTokenAsync(rejected);
                Assert.That(refreshed.access_token, Is.EqualTo("fresh-token"));
                Assert.That(handler.TokenCalls, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task SynchronousLookupObservesAnAsynchronouslyRotatedToken()
        {
            var handler = new FakeHandler((request, token) => Task.FromResult(request.Method == HttpMethod.Get ? AccountResponse() : TokenHttpResponse()));
            using (var transport = new HttpClient(handler))
            {
                var client = CreateClient(transport);
                var provider = new TeamworkOAuthInfoProvider(client);
                var old = provider.GetOAuthInfo("fixture-user-id");
                var errors = new TeamworkClientErrorHandler("fixture-login", provider);
                await errors.RefreshTokenAsync(old);
                var current = provider.GetOAuthInfo("fixture-user-id");
                Assert.That(current.access_token, Is.EqualTo("fresh-token"));
                Assert.That(current.refresh_token, Is.EqualTo("fresh-refresh"));
                Assert.That(current.UserId, Is.EqualTo(old.UserId));
                Assert.That(current.Email, Is.EqualTo(old.Email));
                Assert.That(current.TenantId, Is.EqualTo(old.TenantId));
                Assert.That(handler.Calls, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task SynchronousRefreshRenewsAndReusesTheSharedSession()
        {
            var handler = new FakeHandler((request, token) => Task.FromResult(request.Method == HttpMethod.Get ? AccountResponse() : TokenHttpResponse()));
            using (var transport = new HttpClient(handler))
            {
                var client = CreateClient(transport);
                var provider = new TeamworkOAuthInfoProvider(client);
                var errors = new TeamworkClientErrorHandler("fixture-login", provider);
                var rejected = new OAuthInfo { access_token = "old-token" };
                var current = errors.RefreshToken(rejected);
                Assert.That(current.access_token, Is.EqualTo("fresh-token"));
                Assert.That(errors.RefreshToken(rejected).access_token, Is.EqualTo("fresh-token"));
                Assert.That((await provider.GetOAuthInfoAsync("fixture-user-id")).access_token, Is.EqualTo("fresh-token"));
                Assert.That(client.Config.AccessToken, Is.EqualTo("fresh-token"));
                Assert.That(handler.TokenCalls, Is.EqualTo(1));
                Assert.That(handler.Calls, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task ConcurrentSynchronousAndAsynchronousRefreshesAdmitOneGrant()
        {
            var entered = Signal();
            var release = Signal();
            var syncEntered = Signal();
            var handler = new FakeHandler(async (request, token) =>
            {
                if (request.Method == HttpMethod.Get) return AccountResponse();
                entered.TrySetResult(true);
                await release.Task;
                return TokenHttpResponse();
            });
            using (var transport = new HttpClient(handler))
            {
                var client = CreateClient(transport);
                var first = new TeamworkClientErrorHandler("fixture-login", new TeamworkOAuthInfoProvider(client));
                var second = new TeamworkClientErrorHandler("fixture-login", new TeamworkOAuthInfoProvider(client));
                var rejected = new OAuthInfo { access_token = "old-token" };
                var owner = first.RefreshTokenAsync(rejected);
                await entered.Task;
                var follower = Task.Run(() => { syncEntered.SetResult(true); return second.RefreshToken(rejected); });
                await syncEntered.Task;
                release.SetResult(true);
                var values = await Task.WhenAll(owner, follower);
                Assert.That(handler.TokenCalls, Is.EqualTo(1));
                Assert.That(handler.Calls, Is.EqualTo(2));
                Assert.That(values[0].access_token, Is.EqualTo("fresh-token"));
                Assert.That(values[1].access_token, Is.EqualTo("fresh-token"));
                Assert.That(values[1].UserId, Is.EqualTo("fixture-user-id"));
                Assert.That(values[1].TenantId, Is.EqualTo("fixture-tenant"));
                Assert.That(values[1].Email, Is.EqualTo("fixture-login"));
            }
        }

        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        public async Task FailedSynchronousRefreshPreservesTheTokenAndAllowsAsynchronousRecovery(HttpStatusCode status)
        {
            var attempt = 0;
            var handler = new FakeHandler((request, token) =>
            {
                if (request.Method == HttpMethod.Get) return Task.FromResult(AccountResponse());
                if (++attempt == 1) return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent("{\"error\":\"invalid_grant\"}") });
                return Task.FromResult(TokenHttpResponse());
            });
            using (var transport = new HttpClient(handler))
            {
                var client = CreateClient(transport);
                var errors = new TeamworkClientErrorHandler("fixture-login", new TeamworkOAuthInfoProvider(client));
                var rejected = new OAuthInfo { access_token = "old-token" };
                var error = Assert.Throws<ApiException>((Action)(() => { errors.RefreshToken(rejected); }));
                Assert.That(error.ErrorCode, Is.EqualTo((int)status));
                Assert.That(client.Token.AccessToken, Is.EqualTo("old-token"));
                Assert.That(client.Token.RefreshToken, Is.EqualTo("old-refresh"));
                Assert.That((await errors.RefreshTokenAsync(rejected)).access_token, Is.EqualTo("fresh-token"));
                Assert.That(handler.TokenCalls, Is.EqualTo(2));
            }
        }

        [Test]
        public void FailedSynchronousAccountLookupCanRecover()
        {
            var count = 0;
            var handler = new FakeHandler((request, token) => Task.FromResult(++count == 1 ? JsonResponse("{\"user\":null}") : AccountResponse()));
            using (var transport = new HttpClient(handler))
            {
                var provider = new TeamworkOAuthInfoProvider(CreateClient(transport));
                Assert.Throws<InvalidOperationException>((Action)(() => { provider.GetOAuthInfo("fixture-user-id"); }));
                Assert.That(provider.GetOAuthInfo("fixture-user-id").Email, Is.EqualTo("fixture-login"));
                Assert.That(handler.Calls, Is.EqualTo(2));
            }
        }

        private static TaskCompletionSource<bool> Signal() => new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        private static OpenScopeApiClient CreateClient(HttpClient transport)
        {
            var result = new OpenScopeApiClient(new Configuration { HttpClient = transport, ClientNumber = "fixture-customer", AccessToken = "old-token" });
            result.Token = new TokenResponse(TokenResponse.TokenTypeEnum.Bearer, "old-token", 3600, "old-refresh", "fixture-user-id", 1, "fixture-org", "fixture-tenant");
            return result;
        }
        private static HttpResponseMessage AccountResponse() => JsonResponse("{\"user\":{\"uid\":\"fixture-account-id\",\"login\":\"fixture-login\"}}");
        private static HttpResponseMessage TokenHttpResponse() => JsonResponse("{\"token_type\":\"bearer\",\"access_token\":\"fresh-token\",\"expires_in\":3600,\"refresh_token\":\"fresh-refresh\",\"uid\":\"fixture-user-id\",\"organisationId\":1,\"organisationName\":\"fixture-org\",\"teamworkTenantId\":\"fixture-tenant\"}");
        private static HttpResponseMessage JsonResponse(string json) => new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json") };
        private sealed class FakeHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send;
            internal int Calls;
            internal int TokenCalls;
            internal string LastTokenBody;
            internal FakeHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) { this.send = send; }
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
            {
                Interlocked.Increment(ref Calls);
                if (request.Method == HttpMethod.Post)
                {
                    Interlocked.Increment(ref TokenCalls);
                    LastTokenBody = await request.Content.ReadAsStringAsync();
                }
                return await send(request, token);
            }
        }
    }
}
