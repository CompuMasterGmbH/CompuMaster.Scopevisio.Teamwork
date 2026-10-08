using CenterDevice.Rest;
using CenterDevice.Rest.Clients.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.CenterDeviceApi
{
    public partial class TeamworkOAuthInfoProvider : CenterDevice.Rest.Clients.OAuth.IOAuthInfoProvider
    {
        /// <summary>
        /// Configuration for accessing OpenScope API
        /// </summary>
        public TeamworkOAuthInfoProvider(CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient openscopeClient)
        {
            this.OpenscopeClient = openscopeClient;
        }

        public CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient OpenscopeClient { get; set; }

        /// <inheritdoc/>
        /// <remarks>Uses the current installed token and the same session admission as asynchronous callers. Account identity is cached per OpenScope client; OAuth information is rebuilt after token rotation.</remarks>
        public OAuthInfo GetOAuthInfo(string userId)
        {
            return GetOAuthInfoCoreAsync(System.Threading.CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
