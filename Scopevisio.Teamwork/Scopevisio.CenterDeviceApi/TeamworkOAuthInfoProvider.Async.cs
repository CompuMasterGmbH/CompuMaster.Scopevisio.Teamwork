using CenterDevice.Rest.Clients.OAuth;
using CompuMaster.Scopevisio.OpenApi;
using CompuMaster.Scopevisio.OpenApi.Model;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.CenterDeviceApi
{
    public partial class TeamworkOAuthInfoProvider : IAsyncOAuthInfoProvider
    {
        private sealed class AuthorizationContext
        {
            internal readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);
            internal AccountInfo Account;
        }

        // All adapters using the same token owner share account lookup and refresh.
        private static readonly ConditionalWeakTable<OpenScopeApiClient, AuthorizationContext> Contexts =
            new ConditionalWeakTable<OpenScopeApiClient, AuthorizationContext>();

        /// <inheritdoc/>
        /// <remarks>Uses the installed token and caches the account login per OpenScope client. Callers must not mutate credentials or tokens concurrently outside these asynchronous operations.</remarks>
        public async Task<OAuthInfo> GetOAuthInfoAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var client = OpenscopeClient ?? throw new InvalidOperationException("An OpenScope client is required.");
            var context = Contexts.GetValue(client, key => new AuthorizationContext());
            await context.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await EnsureAccountAsync(client, context, cancellationToken).ConfigureAwait(false);
                return CreateOAuthInfo(client, context);
            }
            finally { context.Gate.Release(); }
        }

        internal async Task<AccountInfo> GetApplicationContextAsync(CancellationToken cancellationToken)
        {
            var client = OpenscopeClient ?? throw new InvalidOperationException("An OpenScope client is required.");
            var context = Contexts.GetValue(client, key => new AuthorizationContext());
            await context.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await EnsureAccountAsync(client, context, cancellationToken).ConfigureAwait(false);
                return context.Account;
            }
            finally { context.Gate.Release(); }
        }

        internal async Task<OAuthInfo> RefreshOAuthInfoAsync(OAuthInfo rejected, CancellationToken cancellationToken)
        {
            var client = OpenscopeClient ?? throw new InvalidOperationException("An OpenScope client is required.");
            var context = Contexts.GetValue(client, key => new AuthorizationContext());
            await context.Gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (client.Token == null) throw new InvalidOperationException("Authorization is required before token refresh.");
                // A concurrent request may already have replaced this rejected token.
                if (rejected == null || string.Equals(client.Token.AccessToken, rejected.access_token, StringComparison.Ordinal))
                    await client.RefreshAuthorizationAsync(cancellationToken).ConfigureAwait(false);
                await EnsureAccountAsync(client, context, cancellationToken).ConfigureAwait(false);
                return CreateOAuthInfo(client, context);
            }
            finally { context.Gate.Release(); }
        }

        private static async Task EnsureAccountAsync(OpenScopeApiClient client, AuthorizationContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (client.Token == null) throw new InvalidOperationException("Authorization is required before Teamwork access.");
            if (context.Account != null) return;
            var response = await client.AdditionalApi.GetApplicationContextAsyncWithHttpInfo(cancellationToken).ConfigureAwait(false);
            if (response.Data?.User == null || string.IsNullOrEmpty(response.Data.User.Login))
                throw new InvalidOperationException("The account response does not contain the authenticated user's login.");
            context.Account = response.Data;
        }

        private static OAuthInfo CreateOAuthInfo(OpenScopeApiClient client, AuthorizationContext context) =>
            new TeamworkOAuthInfo(context.Account.User.Login, client.Token);
    }
}
