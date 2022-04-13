using CenterDevice.Rest;
using CenterDevice.Rest.Clients.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.CenterDeviceApi
{
    public class TeamworkClientErrorHandler : CenterDevice.Rest.Clients.IRestClientErrorHandler
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

        public OAuthInfo RefreshToken(OAuthInfo oAuthInfo)
        {
            return new TeamworkOAuthInfo(this.LoginEMailAddress, this.OAuthProvider.OpenscopeClient.Token);
        }

        public void ValidateResponse(global::RestSharp.RestResponse result)
        {
            if (result.StatusCode >= System.Net.HttpStatusCode.InternalServerError)
                throw new System.Net.WebException("Server error", System.Net.WebExceptionStatus.UnknownError);
        }
    }
}