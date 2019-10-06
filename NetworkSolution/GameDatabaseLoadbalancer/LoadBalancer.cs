using GlobalVariablesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameDatabaseLoadbalancer
{
    public class LoadBalancer
    {
        TcpListener listener;

        public LoadBalancer()
        {
            listener = new TcpListener(IPAddress.Any, GlobalVariables.GAME_DATABASE_LOADBALANCER_PORT);


        }

        void ListenForConnections()
        {
            while(true)
            {
                TcpClient client =  listener.AcceptTcpClient();
            }
        }
    }
}
