using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Login_Middleware
{
    /// <summary>
    /// Middleware_Client represents a connected client internally
    /// </summary>
    class Middleware_Client
    {
        TcpClient tcpClient;

        public Middleware_Client(TcpClient client) 
        {
            tcpClient = client;
        }

    }
}
