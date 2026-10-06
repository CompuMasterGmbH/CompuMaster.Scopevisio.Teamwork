using CenterDevice.Rest.Clients;
using CenterDevice.Rest.Clients.OAuth;
using System.Threading;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.CenterDeviceApi
{
    public partial class TeamworkClientErrorHandler : IAsyncRestClientErrorHandler
    {
        /// <inheritdoc/>
        /// <remarks>Refreshes the OpenScope token once per rejected token owner and reuses an already refreshed token for other concurrent failures.</remarks>
        public Task<OAuthInfo> RefreshTokenAsync(OAuthInfo oAuthInfo, CancellationToken cancellationToken = default(CancellationToken)) =>
            OAuthProvider.RefreshOAuthInfoAsync(oAuthInfo, cancellationToken);
    }
}
