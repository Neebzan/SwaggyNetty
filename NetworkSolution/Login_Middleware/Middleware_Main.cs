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
using System.Threading;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace Login_Middleware
{
    class Middleware_Main
    {
        static int port = 13005;
        static ConcurrentQueue<TcpClient> users = new ConcurrentQueue<TcpClient>();
        
        static private IPAddress IP = IPAddress.Any;
        static public TcpListener serverListener = new TcpListener(IP, port);
        static void Main(string[] args)
        {
            // Start Listen for Clients
            Task.Factory.StartNew(ListenForClients, TaskCreationOptions.LongRunning);

            // Work With Clients
            Task.Factory.StartNew(WaitForClients, TaskCreationOptions.LongRunning);

            while (true)
            {
                
            }

        }
         /// <summary>
         /// 
         /// </summary>
        public static void WaitForClients()
        {
            Console.WriteLine("Waiting For Clients...");

            while (true)
            {
                while (users.Count > 0)
                {
                    if (users.TryDequeue(out TcpClient tcpClient))
                    {
                        Middleware_Client client = new Middleware_Client(tcpClient);
                        Console.WriteLine($"User at IP: {tcpClient.Client.RemoteEndPoint} Recieved Queue Time, Processing requests...");
                        
                        Task.Factory.StartNew(client.ListenForMessages);
                    }
                }
            }
        }

        /// <summary>
        /// Endless Loop that listens for incoming connections,
        /// and puts them as new clients into a concurrent queue.
        /// </summary>
        public static void ListenForClients()
        {
            Console.WriteLine("Starting Server Listen");
            serverListener.Start();

            while (true)
            {
                if (serverListener.Pending())
                {
                    TcpClient c = serverListener.AcceptTcpClient();
                    users.Enqueue(c);
                    Console.WriteLine(c.Client.RemoteEndPoint.ToString() + " connected");
                }
            }
        }
    }
}
