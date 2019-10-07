using GlobalVariablesLib;
using MSMQHelperUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Middleware {
    class Middleware_Main {
        // Listener Fields
        private static int port = GlobalVariables.MIDDLEWARE_PORT;
        private static IPAddress address = IPAddress.Any;
        private static TcpListener serverListener = new TcpListener(address, port);

        // Queue Fields
        private static ConcurrentQueue<TcpClient> enqueuedClients = new ConcurrentQueue<TcpClient>();
        public static ConcurrentQueue<Message> dataBaseResponses = new ConcurrentQueue<Message>();
        public static ConcurrentQueue<Message> tokenResponses = new ConcurrentQueue<Message>();
        public static MessageQueue databaseRequestQueue, databaseResponseQueue, tokenRequestQueue, tokenResponseQueue, invalidLetterQueue, deadLetterQueue;

        // General Fields
        public static double MaxMessageLifeTime { get; private set; } = 4.5;
        private static double tickrate = TimeSpan.FromMilliseconds(16.666666667).TotalMilliseconds;
        private static bool isAlive = true;
        private static Mutex mqMutex = new Mutex();

        static void Main(string[] args) {
            // Sets name of main Thread for debugging purposes
            Thread.CurrentThread.Name = "MAIN_THREAD";

            // Setting or Creating(if missing) messaging queues
            databaseRequestQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.CONSUMER_QUEUE_NAME);
            databaseResponseQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.PRODUCER_QUEUE_NAME);
            tokenRequestQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.TOKEN_INPUT_QUEUE_NAME);
            tokenResponseQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.TOKEN_RESPONSE_QUEUE_NAME);
            invalidLetterQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.INVALID_LETTER_QUEUE);
            deadLetterQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.DEAD_LETTER_QUEUE);

            // purging messaging queues, to avoid lingering messages after a crash or new startup
            databaseRequestQueue.Purge();
            tokenRequestQueue.Purge();
            databaseResponseQueue.Purge();
            tokenResponseQueue.Purge();
            
            // Sets property filters to all be set, to enable sent & recieved times on messages.
            databaseRequestQueue.MessageReadPropertyFilter.SetAll();
            tokenRequestQueue.MessageReadPropertyFilter.SetAll();
            databaseResponseQueue.MessageReadPropertyFilter.SetAll();
            tokenResponseQueue.MessageReadPropertyFilter.SetAll();

            // Start Server Thread
            Task.Factory.StartNew(EnqueueConnection);

            // Wait Time For Correct Order
            Thread.Sleep(TimeSpan.FromMilliseconds(tickrate));

            // Start Client Handler Thread
            Task.Factory.StartNew(DequeueConnection);

            // Wait Time For Correct Order
            Thread.Sleep(TimeSpan.FromMilliseconds(tickrate));

            // Start Incoming Messages Handler for Database Response Queue
            Task.Factory.StartNew(() => DequeueMessage(databaseResponseQueue, dataBaseResponses));

            // Wait Time For Correct Order
            Thread.Sleep(TimeSpan.FromMilliseconds(tickrate));

            // Start Incoming Messages Handler for Database Response Queue
            Task.Factory.StartNew(() => DequeueMessage(tokenResponseQueue, tokenResponses));

            Console.ReadLine();

        }

        /// <summary>
        /// Sends a Message to target queue with a specific label, used to firsthand recognise
        /// </summary>
        /// <param name="request"></param>
        /// <param name="label"></param>
        /// <param name="targetQueue"></param>
        public static void EnqueueMessage(Message request, string label, MessageQueue targetQueue) {
            WriteLine($"Sent A request To {targetQueue.QueueName}\n", true);
            request.TimeToBeReceived = TimeSpan.FromSeconds(MaxMessageLifeTime);
            request.TimeToReachQueue = TimeSpan.FromSeconds(MaxMessageLifeTime);
            request.UseDeadLetterQueue = true;
            request.Formatter = new JsonMessageFormatter();
            targetQueue.Send(request, label);
        }

        /// <summary>
        /// Tries to dequeue a message to be sent through system, or to a user, withing a specified amount of time.
        /// Also checks age of message, and tries to remove old messages.</summary>
        /// <para>Returns true if it was successful in fetching a message, else it returns false</para>
        /// <param name="label">Expected Label to match Message With</param>
        /// <param name="uniqueClientID">Unique Client ID</param>
        /// <param name="targetQueue">Queue To recieve Message From</param>
        /// <param name="correctMessage">Presumed Correct Message From Target Queue, is null if method returns false</param>
        /// <returns></returns>
        public static bool GetMessage(string label, string uniqueClientID, ConcurrentQueue<Message> targetQueue, out Message correctMessage) {
            Stopwatch timeAlive = new Stopwatch();
            timeAlive.Start();
            while (timeAlive.Elapsed.TotalSeconds < MaxMessageLifeTime) {
                // Peek top item until successful
                if (targetQueue.TryPeek(out Message peek)) {
                    // Check if label matches expected id
                    if (peek.Label == label) {
                        // Locks mutex as we expect a user to only have one request submitted
                        mqMutex.WaitOne();
                        try {
                            // Converting Message to get unique connection data
                            UserModel peekedModel = JsonConvert.DeserializeObject<UserModel>(peek.Body.ToString());
                            // To make absolutely sure it's the correct message, and a user does not have more requests, we check the remote endpoint
                            if (peekedModel.RemoteEndPoint == uniqueClientID) {
                                // TryDequeue until Successful, or Successful and Message Too Old
                                while (true) {
                                    // As we now know the presumed message is correct, we pull it out,
                                    // unless it's too old.
                                    if (peek.SentTime.Subtract(DateTime.Now).TotalSeconds < MaxMessageLifeTime && targetQueue.TryDequeue(out correctMessage)) {
                                        WriteLine("Message Sucessfully Dequeued!", true);
                                        mqMutex.ReleaseMutex();
                                        return true;
                                    }
                                    // If the Message Is too old, the Client Connection Has been closed clientside, and the message should not be used.
                                    else if (peek.ArrivedTime.Subtract(DateTime.Now).TotalSeconds > MaxMessageLifeTime) {
                                        // we release the mutex, and return null and false.
                                        mqMutex.ReleaseMutex();
                                        correctMessage = null;
                                        return false;
                                    }
                                }
                            } else {
                                // If not correct Endpoint, release Mutex
                                mqMutex.ReleaseMutex();
                            }
                        } catch (Exception e) {
                            bool success = false;
                            // If we catch an Error, it's presumed that somehting in the message was corrupt, or wrong format.
                            while (!success) {
                                if (targetQueue.TryDequeue(out Message moveToInvalidLetter)) {
                                    EnqueueMessage(moveToInvalidLetter, moveToInvalidLetter.Label, invalidLetterQueue);
                                    WriteLine("Error: " + e.Message, true);
                                    mqMutex.ReleaseMutex();
                                    success = true;
                                }
                            }
                        }
                    } else {
                        // if label is incorrect, the message might be too old,
                        // so we check if peeked message is old. If not, simply another process is waiting for access to said message.
                        if (peek.SentTime.Subtract(DateTime.Now).TotalSeconds > MaxMessageLifeTime) {
                            // Then if mutex is not in use, locks the mutex
                            if (mqMutex.WaitOne(0)) {
                                // Bool to keep track of success.
                                bool success = false;
                                // Tries to remove message, until successful.
                                while (!success) {
                                    if (targetQueue.TryDequeue(out Message moveToDeadLetter)) {
                                        EnqueueMessage(moveToDeadLetter, moveToDeadLetter.Label, deadLetterQueue);
                                        mqMutex.ReleaseMutex();
                                        success = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            timeAlive.Stop();
            // Operation has timedout, and will retun false
            correctMessage = null;
            return false;
        }

        /// <summary>
        /// Dequeues Pending Requests from a MSMQ-queue, and puts them into a local concurrant queue.
        /// </summary>
        /// <param name="targetRecievingQueue"></param>
        /// <param name="targetHandlingQueue"></param>
        private static void DequeueMessage(MessageQueue targetRecievingQueue, ConcurrentQueue<Message> targetHandlingQueue) {
            Thread.CurrentThread.Name = $"MESSAGE_HANDLER_[{targetRecievingQueue.QueueName.ToUpper()}]";
            WriteLine("Handler Started");
            while (isAlive) {
                try {
                    Message response = targetRecievingQueue.Receive();
                    WriteLine($"Message Recieved from {targetRecievingQueue.QueueName}", true);
                    // Sets formatter to Json Message Formatter for ease of use later.
                    response.Formatter = new JsonMessageFormatter();

                    // Enqueues into local queue
                    targetHandlingQueue.Enqueue(response);

                } catch (Exception e) {
                    WriteLine(e.Message, true);
                }
                Thread.Sleep(1);
            }
        }
        #region  WriteLine Methods
        /// <summary>
        /// Generalised method to print, to keep message format uniform across middleware classes.
        /// Also provides a time stamp
        /// </summary>
        /// <param name="value">string value to be printed to the screen</param>
        public static void WriteLine(string value) {
            string threadName = Thread.CurrentThread.Name;
            string date = DateTime.Now.ToString("h:mm:ss tt");
            string message = $"{date} {threadName}: {value}";
            Console.WriteLine(message);
        }

        /// <summary>
        /// <para>Generalised method to print, to keep message format uniform across middleware classes.
        /// Also provides a time stamp</para>
        /// </summary>
        /// <param name="value">string value to be printed to the screen</param>
        /// <param name="debugOnly">determines whether messages are printed in debug mode only</param>
        public static void WriteLine(string value, bool debugOnly) {
            if (debugOnly) {
#if DEBUG
                string threadName = Thread.CurrentThread.Name;
                string date = DateTime.Now.ToString("h:mm:ss tt");
                string message = $"{date} {threadName}: {value}";
                Console.WriteLine(message);
#endif
            } else {
                string threadName = Thread.CurrentThread.Name;
                string date = DateTime.Now.ToString("h:mm:ss tt");
                string message = $"{date} {threadName}: {value}";
                Console.WriteLine(message);
            }

        }
        #endregion

        /// <summary>
        /// Starts listening with TCPListener and awaits pending TCP Connections and enqueues them to local Concurrant Queue, to be handled otherwise.
        /// Ticks 60 times a second.
        /// </summary>
        static void EnqueueConnection() {
            // Sets The Current Thread Name, as the Enqueue Connection method is intended to be run in a seperate Thread.
            Thread.CurrentThread.Name = $"SERVER[{serverListener.LocalEndpoint.ToString()}]";

            // Debug Info
            WriteLine("Starting Listener", false);

            // Starts Server
            serverListener.Start();

            // While program is supposed to be running, if there is a pending connection, 
            // create a TcpClient to as pending connection, and enqueue said client.
            while (isAlive) {
                if (serverListener.Pending()) {
                    TcpClient pendingClient = serverListener.AcceptTcpClient();
                    enqueuedClients.Enqueue(pendingClient);
                    WriteLine("SERVER: " + pendingClient.Client.RemoteEndPoint.ToString() + " connected", false);
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(tickrate));
            }
        }
        /// <summary>
        /// While program is running, 120 times a second, dequeues connections into ClientRequestHandler(s) as new tasks to run multithreaded
        /// </summary>
        static void DequeueConnection() {
            // Sets The Current Thread Name, as the Dequeue Connection method is intended to be run in a seperate Thread.
            Thread.CurrentThread.Name = $"CLIENT_HANDLER[{serverListener.LocalEndpoint.ToString()}]";

            // Debug info
            WriteLine("Starting Client Handler", false);

            // While program is supposed to be running, if the queue isn't empty, 
            // try dequeuing as and create a seperate thread for a client
            while (isAlive) {
                if (enqueuedClients.Count > 0) {
                    if (enqueuedClients.TryDequeue(out TcpClient connection)) {
                        ClientRequestHandler client = new ClientRequestHandler(connection);
                        Task.Factory.StartNew(client.HandleRequest);
                        WriteLine("Dequeued Client into new ClientRequestHandler", false);
                    }
                }
                Thread.Sleep(TimeSpan.FromMilliseconds(tickrate / 2));
            }
        }
    }
}