using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GlobalVariablesLib
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
