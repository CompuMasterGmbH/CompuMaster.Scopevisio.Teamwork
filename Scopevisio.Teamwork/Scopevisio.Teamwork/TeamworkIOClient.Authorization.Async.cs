using CenterDevice.Rest.Clients;
using CompuMaster.Scopevisio.OpenApi;
using CompuMaster.Scopevisio.OpenApi.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.Teamwork
{
    public partial class TeamworkIOClient
    {
        /// <summary>Authorizes a configured user and initializes Teamwork using cancellable asynchronous HTTP I/O.</summary>
        /// <param name="configuration">A fresh caller-owned configuration with its HTTP client created by <see cref="CenterDeviceHttpTransport.CreateHttpClient"/> before its first API request.</param>
        /// <param name="cancellationToken">Cancels authorization, account lookup, or waiting for request admission.</param>
        /// <returns>The initialized client. The caller retains ownership of the configured HTTP client.</returns>
        /// <remarks>The shared transport is required to apply the same per-origin policy to OpenScope authorization and Teamwork I/O. Existing synchronous constructors and directory members retain their behavior.</remarks>
        /// <exception cref="ArgumentNullException">The configuration is null.</exception>
        /// <exception cref="ArgumentException">No HTTP client is configured.</exception>
        public static async Task<TeamworkIOClient> AuthorizeAsync(Configuration configuration, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (configuration.HttpClient == null) throw new ArgumentException("A caller-owned shared HTTP transport must be configured before authorization.", nameof(configuration));
            var client = new OpenScopeApiClient(configuration);
            await client.AuthorizeWithUserCredentialsAsync(cancellationToken).ConfigureAwait(false);
            return await CreateAsync(client, cancellationToken).ConfigureAwait(false);
        }
    }
}
