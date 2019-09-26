using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSMQHelperUtilities;
using GlobalVariablesLib;
using System.Messaging;
using JWTlib;
using System.Net.Sockets;
using TcpHelper;
using System.Net;
using System.Threading;

namespace TestProject
{
    class Program
    {
        static MessageQueue userInputMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.TOKEN_INPUT_QUEUE_NAME);
        static MessageQueue beaconInputMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.BEACON_INPUT_QUEUE_NAME);
        static MessageQueue beaconResponseMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.BEACON_RESPONSE_QUEUE_NAME);
        static MessageQueue testMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.TEST_QUEUE_NAME);

        static string token = string.Empty;

        static void Main(string[] args)
        {
            bool exit = false;
            TcpClient beaconClient = null;

            while (!exit)
            {                
                Console.WriteLine("1. Request new JWT Token");
                Console.WriteLine("2. Connect to TokenSystem - Tcp");
                if (beaconClient == null)
                    Console.WriteLine("3. Connect to beacon - Tcp");
                else
                    Console.WriteLine("3. Disconnect from beacon - Tcp");
                Console.WriteLine("Esc to quit");
                ConsoleKey key = Console.ReadKey().Key;

                switch (key)
                {
                    case ConsoleKey.D1:
                        {
                            Console.Clear();
                            RequestJWTToken();
                            break;
                        }
                    case ConsoleKey.D2:
                        {
                            Console.Clear();
                            ConnectoToTokenSystemTcp();
                            break;
                        }
                    case ConsoleKey.D3:
                        {
                            Console.Clear();
                            if (beaconClient == null)
                            {
                                Console.WriteLine("Trying to connect");
                                beaconClient = new TcpClient("127.0.0.1", GlobalVariables.BEACON_PORT);
                                Console.WriteLine("Connected to beacon");
                            }
                            else
                            {
                                beaconClient.Close();
                                beaconClient.Dispose();
                                beaconClient = null;
                            }
                            break;
                        }
                    case ConsoleKey.Escape:
                        {
                            exit = true;
                            break;
                        }
                }

            }
        }

        static void RequestJWTToken()
        {
            Console.Write("Username:");
            string username = Console.ReadLine();
            UserModel userModel = new UserModel() { UserID = username };

            Console.WriteLine("Sending request to token system");
            MSMQHelper.SendMessage(userInputMQ, userModel, TokenRequestType.CreateToken.ToString(), testMQ);

            Console.WriteLine("Awaiting response in testMQ");
            Message msg = MSMQHelper.ReceiveMessage(testMQ);
            msg.Formatter = new JsonMessageFormatter();

            token = MSMQHelper.GetMessageBody<string>(msg);

            JWTPayload payload = JWTManager.GetModelFromToken<JWTPayload>(token);


            //JWTPayload payload = MSMQHelper.GetMessageBody<JWTPayload>(msg);


            Console.WriteLine("Response received");
            Console.WriteLine("Printing payload:");
            Console.WriteLine("UserID = " + payload.User.UserID);
            Console.WriteLine("Servers:");
            foreach (var item in payload.ServersInfo.Servers)
            {
                Console.WriteLine(item.IP + ":" + item.Port);
            }

        }

        static void ConnectoToTokenSystemTcp()
        {
            Console.WriteLine("Trying to connect");
            TcpClient client = new TcpClient("127.0.0.1", GlobalVariables.TOKENSYSTEM_PORT);
            Console.WriteLine("Connected to token system");
            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("1. Send invalid token");
                if (token != string.Empty)
                    Console.WriteLine("2. Send valid token");
                else
                {
                    Console.WriteLine("2. Request a new token");
                }
                Console.WriteLine("Escape to go back");
                ConsoleKey key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.D1:
                        {
                            Console.Clear();
                            byte[] data = MessageFormatter.MessageBytes("Invalid token");
                            client.GetStream().Write(data, 0, data.Length);
                            Console.WriteLine("Invalid token sent to TokenSystem");
                            Console.WriteLine("Awaiting response");
                            Thread.Sleep(200);
                            string txt = string.Empty;

                            txt = MessageFormatter.ReadMessage(client.GetStream());
                            //Enum.TryParse(MessageFormatter.ReadMessage(client.GetStream()), out response);
                            Console.WriteLine("Response received: {0}", txt);
                            break;
                        }
                    case ConsoleKey.D2:
                        {
                            Console.Clear();
                            if (token == string.Empty)
                                RequestJWTToken();
                            else
                            {
                                byte[] data = MessageFormatter.MessageBytes(token);
                                client.GetStream().Write(data, 0, data.Length);
                                Console.WriteLine("Valid token sent to TokenSystem");
                                Console.WriteLine("Awaiting response");
                                Thread.Sleep(200);
                                string txt = string.Empty;

                                txt = MessageFormatter.ReadMessage(client.GetStream());
                                //Enum.TryParse(MessageFormatter.ReadMessage(client.GetStream()), out response);
                                Console.WriteLine("Response received: {0}", txt);
                            }
                            break;
                        }
                    case ConsoleKey.Escape:
                        {
                            Console.Clear();
                            exit = true;
                            break;
                        }
                }
            }


        }
    }
}
