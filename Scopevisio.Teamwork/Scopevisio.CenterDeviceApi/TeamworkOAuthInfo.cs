using CenterDevice.Rest;
using CenterDevice.Rest.Clients.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.CenterDeviceApi
{
    public class TeamworkOAuthInfo : global::CenterDevice.Rest.Clients.OAuth.OAuthInfo
    {
        public TeamworkOAuthInfo(string loginEMailAddress, CompuMaster.Scopevisio.OpenApi.Model.TokenResponse openscopeToken)
        {
            this.Email = loginEMailAddress;
            this.token_type = openscopeToken.TokenType.ToString();
            this.TenantId = openscopeToken.TeamworkTenantId;
            this.UserId = openscopeToken.Uid;
            this.access_token = openscopeToken.AccessToken;
            this.refresh_token = openscopeToken.RefreshToken;
            this.expires_in = openscopeToken.ExpiresIn;
        }
    }
}