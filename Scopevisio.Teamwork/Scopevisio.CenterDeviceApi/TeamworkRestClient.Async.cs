using CompuMaster.Scopevisio.OpenApi;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.CenterDeviceApi
{
    public partial class TeamworkRestClient
    {
        /// <summary>Creates a Teamwork REST client using cancellable asynchronous account lookup.</summary>
        /// <param name="openscopeClient">An authorized OpenScope client whose transport and lifetime remain owned by the caller.</param>
        /// <param name="cancellationToken">Cancels waiting and the active account request.</param>
        /// <returns>The initialized Teamwork REST client.</returns>
        /// <exception cref="ArgumentNullException">The OpenScope client is null.</exception>
        /// <exception cref="InvalidOperationException">Authorization or an authenticated account login is unavailable.</exception>
        public static async Task<TeamworkRestClient> CreateAsync(OpenScopeApiClient openscopeClient, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (openscopeClient == null) throw new ArgumentNullException(nameof(openscopeClient));
            var provider = new TeamworkOAuthInfoProvider(openscopeClient);
            var account = await provider.GetApplicationContextAsync(cancellationToken).ConfigureAwait(false);
            return new TeamworkRestClient(provider, account);
        }
    }
}
