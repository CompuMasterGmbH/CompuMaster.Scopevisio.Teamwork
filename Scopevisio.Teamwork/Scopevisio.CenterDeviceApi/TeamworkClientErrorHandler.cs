using CenterDevice.Rest;
using CenterDevice.Rest.Clients.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.CenterDeviceApi
{
    public partial class TeamworkClientErrorHandler : CenterDevice.Rest.Clients.IRestClientErrorHandler
    {
        /// <summary>
        /// Configuration for accessing OpenScope API
        /// </summary>
        public TeamworkClientErrorHandler(string loginEMailAddress, TeamworkOAuthInfoProvider oauthProvider)
        {
            this.OAuthProvider = oauthProvider;
            this.LoginEMailAddress = loginEMailAddress;
        }

        public TeamworkOAuthInfoProvider OAuthProvider { get; set; }
        public string LoginEMailAddress { get; set; }

        /// <inheritdoc/>
        /// <remarks>Refreshes through the shared OpenScope session admission and reuses an already rotated token for concurrent failures. Refresh failures are propagated without replacing the installed token.</remarks>
        public OAuthInfo RefreshToken(OAuthInfo oAuthInfo)
        {
            return OAuthProvider.RefreshOAuthInfoAsync(oAuthInfo, System.Threading.CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        /// <remarks>Preserves the HTTP status and transport cause without exposing response bodies or request credentials.</remarks>
        public void ValidateResponse(global::RestSharp.RestResponse result)
        {
            if (result.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
            {
                var error = new System.Net.WebException("Server error (HTTP " + (int)result.StatusCode + ").",
                    result.ErrorException, System.Net.WebExceptionStatus.UnknownError, null);
                error.Data["CompuMaster.Scopevisio.Teamwork.HttpStatusCode"] = (int)result.StatusCode;
                error.Data["CompuMaster.Scopevisio.Teamwork.ResponseStatus"] = result.ResponseStatus.ToString();
                throw error;
            }
        }
    }
}
