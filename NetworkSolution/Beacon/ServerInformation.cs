using System.Collections.Generic;

namespace Beacon
{
    public class ServerInformation
    {
        public string ServerName { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
    }

    public class ServersData
    {
        public List<ServerInformation> Servers { get; set; }
    }
}