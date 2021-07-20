using CenterDevice.Rest;
using CenterDevice.Rest.Clients.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompuMaster.Scopevisio.CenterDeviceApi
{
    public class TeamworkRestClientConfiguration : CenterDevice.Rest.IRestClientConfiguration
    {
        /// <summary>
        /// Configuration for accessing OpenScope API
        /// </summary>
        public TeamworkRestClientConfiguration(string teamworkBaseAddress, string userAgent)
        {
            this.TeamworkBaseAddress = teamworkBaseAddress;
            this.UserAgent = userAgent;
        }

        public string TeamworkBaseAddress { get; set; }

        public string UserAgent { get; set; }

        string IRestClientConfiguration.BaseAddress => this.TeamworkBaseAddress;

        string IRestClientConfiguration.UserAgent => this.UserAgent;
    }
}