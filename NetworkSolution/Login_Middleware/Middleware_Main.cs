using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace Login_Middleware
{
    class Middleware_Main
    {
        static int port = 0;
        static private IPAddress IP = IPAddress.Any;

        public static Queue<Database_Request_Handler> databaseRequests = new Queue<Database_Request_Handler>();
        static ConcurrentQueue<TcpClient> users = new ConcurrentQueue<TcpClient>();
        

        static public TcpListener serverListener = new TcpListener(IP,port);
        static void Main(string[] args)
        {
            // Start Listen for Clients
            Task.Factory.StartNew(ListenForClients, TaskCreationOptions.LongRunning);

            // Work With Clients
            Task.Factory.StartNew(WaitForClients, TaskCreationOptions.LongRunning);

        }
         /// <summary>
         /// 
         /// </summary>
         /// <returns></returns>
        public static IEnumerator WaitForClients()
        {
            while (true)
            {
                while (users.Count > 0)
                {
                    if (users.TryDequeue(out TcpClient tcpClient))
                    {
                        Middleware_Client client = new Middleware_Client(tcpClient);
                        Console.WriteLine($"User at IP: {tcpClient.Client.RemoteEndPoint} Recieved Queue time, Processing requests...");

                        Task.Factory.StartNew(client.ListenForMessages);
                    }
                }
                yield return null;
            }
        }


        public static void ListenForClients()
        {
            serverListener.Start();

            while (true)
            {
                TcpClient c = serverListener.AcceptTcpClient();
                users.Enqueue(c);
                Console.WriteLine(c.Client.RemoteEndPoint.ToString() + " connected");
            }
        }
    }
}
