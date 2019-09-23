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

        public Beacon()
        {
            serverData.Servers = new List<ServerInformation>();

            Thread t = new Thread(ListenForServers);
            t.IsBackground = true;
            t.Start();
        }

        public ServersData GetServerData()
        {
            List<ServerInformation> temp = new List<ServerInformation>();
            foreach (var item in serverData.Servers)
            {
                if (!Connected(item.Client))
                    temp.Add(item);
            }
            foreach (var item in temp)
            {
                serverData.Servers.Remove(item);
            }

            return serverData;
        }

        void ListenForServers()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, GlobalVariables.BEACON_PORT);
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                lock (serverData)
                {
                    serverData.Servers.Add(new ServerInformation()
                    {
                        IP = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(),
                        Port = ((IPEndPoint)client.Client.RemoteEndPoint).Port,
                        Client = client
                    }
                    );
                }
                Console.WriteLine("Connected!");
            }
        }

        public bool Connected(TcpClient tcpClient)
        {
            try
            {
                if (tcpClient.Client != null && tcpClient.Client.Connected)
                {
                    if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];

                        if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            return false;
                        }

                        return true;
                    }

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
