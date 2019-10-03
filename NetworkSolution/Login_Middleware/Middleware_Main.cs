using GlobalVariablesLib;
using MSMQHelperUtilities;
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

namespace Login_Middleware {
    class Middleware_Main {
        static int port = GlobalVariables.MIDDLEWARE_PORT;
        static ConcurrentQueue<TcpClient> users = new ConcurrentQueue<TcpClient>();

        static private IPAddress IP = IPAddress.Any;
        static public TcpListener serverListener = new TcpListener(IP, port);
        public static MessageQueue databaseRequestQueue, databaseResponseQueue, tokenRequestQueue, tokenResponseQueue;

        private static bool isAlive = true;

        static void Main() {
            databaseRequestQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.CONSUMER_QUEUE_NAME);
            databaseResponseQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.PRODUCER_QUEUE_NAME);
            tokenRequestQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.TOKEN_INPUT_QUEUE_NAME);
            tokenResponseQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.TOKEN_RESPONSE_QUEUE_NAME);
            databaseRequestQueue.Purge();
            databaseResponseQueue.Purge();
            tokenRequestQueue.Purge();
            tokenResponseQueue.Purge();

            // Start Listen for Clients
            Task.Factory.StartNew(ListenForClients, TaskCreationOptions.LongRunning);

            // Work With Clients
            Task.Factory.StartNew(WaitForClients, TaskCreationOptions.LongRunning);

            KeepAlive();
        }

        /// <summary>
        /// <para>KeepAlive, static function that expects a ReadLine Command, and
        /// because of this, it locks the main thread so it doesn't use CPU Time</para>
        /// It also provides a way to set the program to terminate.
        /// This will however instantly terminate the console window, so all debug info is lost.
        /// </summary>
        private static void KeepAlive() {
            if (Console.ReadLine() != "TERMINATE") {
                Console.WriteLine("Program is Kept Alive, Type TERMINATE to close window and set to terminate when workload is done");
                KeepAlive();
            } else {
                Console.WriteLine("Program set to Terminate");
                isAlive = false;
            }
        }
        /// <summary>
        /// While Program is running, if any users are in queue, start listening proces
        /// and let the user indirectly queue requests to database and/or token system.
        /// </summary>
        public static void WaitForClients() {
            Console.WriteLine("Waiting For Clients...");

            while (isAlive) {

                if (users.Count > 0) {
                    if (users.TryDequeue(out TcpClient tcpClient)) {
                        Middleware_Client client = new Middleware_Client(tcpClient);
                        Console.WriteLine($"User at IP: {tcpClient.Client.RemoteEndPoint} Recieved Queue Time, Processing requests...");

                        Task.Factory.StartNew(client.ListenForMessages, TaskCreationOptions.PreferFairness);
                    }
                }
                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// Endless Loop that listens for incoming connections,
        /// and puts them as new clients into a concurrent queue.
        /// </summary>
        public static void ListenForClients() {
            Console.WriteLine("Starting Server Listen");
            serverListener.Start();

            while (isAlive) {
                if (serverListener.Pending()) {
                    TcpClient c = serverListener.AcceptTcpClient();
                    users.Enqueue(c);
                    Console.WriteLine("SERVER: " + c.Client.RemoteEndPoint.ToString() + " connected");
                }
                Thread.Sleep(5);
            }

        }
    }
}
