using CenterDevice.Rest.Clients.OAuth;
using System.Threading;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.CenterDeviceApi
{
    public partial class TeamworkOAuthInfoProvider : IAsyncOAuthInfoProvider
    {
        /// <inheritdoc/>
        /// <remarks>Uses the installed token and caches the account login per OpenScope client. Synchronous and asynchronous callers share session admission. Callers must not mutate credentials or tokens concurrently outside these operations.</remarks>
        public Task<OAuthInfo> GetOAuthInfoAsync(string userId, CancellationToken cancellationToken = default(CancellationToken)) =>
            GetOAuthInfoCoreAsync(cancellationToken);
    }
}
