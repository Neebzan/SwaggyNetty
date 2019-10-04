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
        public static MessageQueue databaseRequestQueue, databaseResponseQueue, tokenRequestQueue, tokenResponseQueue, deadLetterQueue, invalidLetterQueue;

        private static bool isAlive = true;

        static void Main() {
            Thread.CurrentThread.Name = "MAIN_THREAD";

            // Main queue setting/initializing
            databaseRequestQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.CONSUMER_QUEUE_NAME);
            databaseResponseQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.PRODUCER_QUEUE_NAME);
            tokenRequestQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.TOKEN_INPUT_QUEUE_NAME);
            tokenResponseQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.TOKEN_RESPONSE_QUEUE_NAME);

            // Dead letter and invalid letter queue setting/initializing
            deadLetterQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.DEAD_LETTER_QUEUE);
            invalidLetterQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.INVALID_LETTER_QUEUE);


            // Purging main queues for old messages
            databaseRequestQueue.Purge();
            databaseResponseQueue.Purge();
            tokenRequestQueue.Purge();
            tokenResponseQueue.Purge();

            // Start Listen for New Client Connections
            Task.Factory.StartNew(ListenForClients, TaskCreationOptions.LongRunning).Wait(100);
            // Work With Clients
            Task.Factory.StartNew(HandleClients, TaskCreationOptions.LongRunning).Wait(100);
            // Setup Old Message Cleanup
            Task.Factory.StartNew(CleanUpMessages).Wait(100);

            // Keep Main Thread Alive
            KeepAlive();
        }


        /// <summary>
        /// WriteLine prints a string message to the console screen using Console.WriteLine function
        /// and adds a time stamp and the thread by name, it was printed from.
        /// </summary>
        /// <param name="message">Represents a string that will be printed to cmd</param>
        public static void WriteLine(string message) {
#if DEBUG
            string date = DateTime.Now.ToString("h:mm:ss tt");
            Console.WriteLine($"{date} {Thread.CurrentThread.Name}: " + message);
#endif
        }

        /// <summary>
        /// <para>KeepAlive, static function that expects a ReadLine Command, and
        /// because of this, it locks the main thread so it doesn't use CPU Time</para>
        /// It also provides a way to set the program to terminate.
        /// This will however instantly terminate the console window, so all debug info is lost.
        /// </summary>
        private static void KeepAlive() {
            if (Console.ReadLine() != "TERMINATE") {
                WriteLine("Program is Kept Alive, Type TERMINATE to close window and set to terminate when workload is done");
                KeepAlive();
            } else {
                WriteLine("Program set to Terminate");
                isAlive = false;
            }
        }

        /// <summary>
        /// Cleans up old messages from relevant recieve queues, to avoid lingering messages blocking the system.
        /// </summary>
        public static void CleanUpMessages() {
            Thread.CurrentThread.Name = "CLEAN_MSG_THREAD";
            WriteLine("Service Started");
            while (isAlive) {
                Thread.Sleep(100);
                Message topMessage = databaseResponseQueue.Peek();
                string identification = topMessage.Id;
                if (DateTime.Now.Subtract(topMessage.SentTime) > TimeSpan.FromSeconds(5)) {
                    try {
                        Message removed = databaseResponseQueue.ReceiveById(identification);
                        WriteLine($"MSG:{removed.Label} FROM: [{GlobalVariables.PRODUCER_QUEUE_NAME}],TIMED OUT AND HAS BEEN REMOVED!");
                        deadLetterQueue.Send(removed);
                    } catch (Exception e) {
                        WriteLine(e.Message + " {\n" + e.ToString() + "\n}");
                    }
                }
                topMessage = tokenResponseQueue.Peek();
                identification = topMessage.Id;
                if (DateTime.Now.Subtract(topMessage.SentTime) > TimeSpan.FromSeconds(5)) {
                    try {
                        Message removed = tokenResponseQueue.ReceiveById(identification);
                        WriteLine($"MSG:{removed.Label} FROM: [{GlobalVariables.TOKEN_RESPONSE_QUEUE_NAME}],TIMED OUT AND HAS BEEN REMOVED!");
                        deadLetterQueue.Send(removed);
                    } catch (Exception e) {
                        WriteLine(e.Message + " {\n" + e.ToString() + "\n}");
                    }
                }
            }
        }

        /// <summary>
        /// Endless Loop that listens for incoming connections,
        /// and puts them as new clients into a concurrent queue.
        /// </summary>
        public static void ListenForClients() {
            Thread.CurrentThread.Name = "AWAIT_CONNECTIONS_THREAD";
            WriteLine("Service Started, awaiting Connections");

            serverListener.Start();

            while (isAlive) {
                if (serverListener.Pending()) {
                    TcpClient c = serverListener.AcceptTcpClient();
                    users.Enqueue(c);
                    WriteLine("SERVER: " + c.Client.RemoteEndPoint.ToString() + " connected");
                }
                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// WaitForClients tries to dequeue awaiting clients
        /// and let the user indirectly queue requests to database and/or token system.
        /// </summary>
        public static void HandleClients() {
            Thread.CurrentThread.Name = "HANDLE_REQUESTS_THREAD";
            WriteLine("Service Started, waiting for enqueued clients");
            while (isAlive) {

                if (users.Count > 0) {
                    if (users.TryDequeue(out TcpClient tcpClient)) {
                        WriteLine($"User at IP: {tcpClient.Client.RemoteEndPoint} Recieved Queue Time, Now Processing requests...");
                        Middleware_Client client = new Middleware_Client(tcpClient);
                        Task.Factory.StartNew(client.ListenForMessages, TaskCreationOptions.PreferFairness);
                    }
                }
                Thread.Sleep(5);
            }
        }

    }
}
