using CompuMaster.Scopevisio.OpenApi;
using System.Threading;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.Teamwork
{
    public partial class TeamworkIOClient
    {
        /// <summary>Creates a Teamwork I/O client using cancellable asynchronous account lookup.</summary>
        /// <param name="openscopeClient">An authorized OpenScope client whose transport and lifetime remain owned by the caller.</param>
        /// <param name="cancellationToken">Cancels waiting and the active account request.</param>
        /// <returns>The initialized client; existing synchronous directory members retain their behavior.</returns>
        public static async Task<TeamworkIOClient> CreateAsync(OpenScopeApiClient openscopeClient, CancellationToken cancellationToken = default(CancellationToken)) =>
            new TeamworkIOClient(await CenterDeviceApi.TeamworkRestClient.CreateAsync(openscopeClient, cancellationToken).ConfigureAwait(false));
    }
}
