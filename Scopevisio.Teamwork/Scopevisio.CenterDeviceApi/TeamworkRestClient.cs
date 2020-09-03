using CenterDevice.Rest;
using CenterDevice.Rest.Clients.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.CenterDeviceApi
{
    /// <summary>
    /// Teamwork client for low level API access to Teamwork's underlying CenterDevice services
    /// </summary>
    public class TeamworkRestClient : CenterDevice.Rest.Clients.CenterDeviceClientBase
    {
        public TeamworkRestClient(CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient openscopeClient)
            : this(new TeamworkOAuthInfoProvider(openscopeClient), openscopeClient.AdditionalApi.GetApplicationContextWithHttpInfo().Data)
        {
            this.OpenscopeClient = openscopeClient;
        }

        private TeamworkRestClient(TeamworkOAuthInfoProvider oauthProvider, CompuMaster.Scopevisio.OpenApi.Model.AccountInfo applicationContext) 
            : base(
                  oauthProvider,
                  new TeamworkRestClientConfiguration(TEAMWORK_ENDPOINT_URL, oauthProvider.OpenscopeClient.Config.UserAgent),
                  new TeamworkClientErrorHandler(applicationContext.User.Login, oauthProvider),
                  ""
                  )
        {
            this.ApplicationContext = applicationContext;
        }

        const string TEAMWORK_ENDPOINT_URL = "https://appload.scopevisio.com/rest/teamworkbridge/";

        public CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient OpenscopeClient { get; }
        public CompuMaster.Scopevisio.OpenApi.Model.AccountInfo ApplicationContext { get; }

        protected override string UploadLinkBaseUrl => "https://upload.teamwork.scopevisio.com/";
    }
}