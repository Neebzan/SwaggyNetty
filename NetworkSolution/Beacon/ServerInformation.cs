using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Beacon
{
    public class ServerInformation
    {
        public string IP { get; set; }
        public int Port { get; set; }
        [JsonIgnore]
        public TcpClient Client { get; set; }
    }

    public class ServersData
    {
        public List<ServerInformation> Servers { get; set; }
    }
}