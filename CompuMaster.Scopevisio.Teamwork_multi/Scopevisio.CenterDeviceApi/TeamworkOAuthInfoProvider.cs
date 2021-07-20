using CenterDevice.Rest;
using CenterDevice.Rest.Clients.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.CenterDeviceApi
{
    public class TeamworkOAuthInfoProvider : CenterDevice.Rest.Clients.OAuth.IOAuthInfoProvider
    {
        /// <summary>
        /// Configuration for accessing OpenScope API
        /// </summary>
        public TeamworkOAuthInfoProvider(CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient openscopeClient)
        {
            this.OpenscopeClient = openscopeClient;
        }

        public CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient OpenscopeClient { get; set; }

        private OAuthInfo getOAuthInfo = null;
        public OAuthInfo GetOAuthInfo(string userId)
        {
            if ((getOAuthInfo == null) || (getOAuthInfo.UserId != userId))
                this.getOAuthInfo = new TeamworkOAuthInfo(OpenscopeClient.AdditionalApi.GetApplicationContextWithHttpInfo().Data.User.Login, OpenscopeClient.Token);
            return this.getOAuthInfo;
        }
    }
}