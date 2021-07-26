using CenterDevice.IO;
    
namespace CompuMaster.Scopevisio.Teamwork
{
    public class TeamworkIOClient : CenterDevice.IO.IOClientBase
    {
        public TeamworkIOClient(string scopevisioCustomerNumber, string username, string password)
            : this(CreateAndAuthorizeOpenScopeApiClientInstance(scopevisioCustomerNumber, username, password))
        {
        }

        public TeamworkIOClient(CompuMaster.Scopevisio.OpenApi.OpenScopeApiClient openscopeClient)
            : this(new CompuMaster.Scopevisio.CenterDeviceApi.TeamworkRestClient(openscopeClient))
        {
        }

        public TeamworkIOClient(CompuMaster.Scopevisio.CenterDeviceApi.TeamworkRestClient teamworkRestClient) : base(teamworkRestClient, teamworkRestClient.OpenscopeClient.Token.Uid)
        {
            this.teamworkRestClient = teamworkRestClient;
        }

        private static OpenApi.OpenScopeApiClient CreateAndAuthorizeOpenScopeApiClientInstance(string scopevisioCustomerNumber, string username, string password)
        {
            OpenApi.OpenScopeApiClient Result = new OpenApi.OpenScopeApiClient();
            Result.AuthorizeWithUserCredentials(username, scopevisioCustomerNumber, password);
            return Result;
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