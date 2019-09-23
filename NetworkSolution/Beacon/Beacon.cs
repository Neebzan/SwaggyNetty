using GlobalVariablesLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Beacon
{
    public class Beacon
    {
        public ServersData serverData = new ServersData();
        public ConcurrentQueue<TcpClient> servers = new ConcurrentQueue<TcpClient>();

        public Beacon()
        {
            serverData.Servers = new List<ServerInformation>();
            serverData.Servers.Add(new ServerInformation() { ServerName = "Test Server1", IP = "Test IP", Port = 13000 });

            Thread t = new Thread(ListenForServers);
            t.IsBackground = true;
            t.Start();
        }

        void ListenForServers()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, GlobalVariables.BEACON_PORT);
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("Connected!");           
            }
        }
    }
}
