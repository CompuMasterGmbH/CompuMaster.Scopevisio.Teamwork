using CenterDevice.IO;
    
namespace CompuMaster.Scopevisio.Teamwork
{
    public class TeamworkIOClient : CenterDevice.IO.IOClientBase
    {
        public TeamworkIOClient(CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient openscopeClient)
            : this(new CompuMaster.Scopevisio.CenterDeviceApi.TeamworkRestClient(openscopeClient))
        {
        }

        public TeamworkIOClient(CompuMaster.Scopevisio.CenterDeviceApi.TeamworkRestClient teamworkRestClient) : base(teamworkRestClient, teamworkRestClient.OpenscopeClient.Token.Uid)
        {
            this.teamworkRestClient = teamworkRestClient;
        }

        private readonly CompuMaster.Scopevisio.CenterDeviceApi.TeamworkRestClient teamworkRestClient;
        public CompuMaster.Scopevisio.CenterDeviceApi.TeamworkRestClient TeamworkRestClient
        {
            get
            {
                return this.teamworkRestClient;
            }
        }
    }
}