using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PatchManagerServer {
    public class PatchClient {
        public TcpClient client;
        public Dictionary<string, string> fileList = null;


        public PatchClient (TcpClient client) {
            this.client = client;
        }
    }
}
