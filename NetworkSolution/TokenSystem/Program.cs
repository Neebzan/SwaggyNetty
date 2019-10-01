﻿using GlobalVariablesLib;
using JWTlib;
using MSMQHelperUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TokenSystem
{
    class Program
    {
        static MessageQueue userInputMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.TOKEN_INPUT_QUEUE_NAME);
        static MessageQueue beaconInputMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.BEACON_INPUT_QUEUE_NAME);
        static MessageQueue beaconResponseMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.BEACON_RESPONSE_QUEUE_NAME);

        static void Main(string[] args)
        {
            Thread t = new Thread(HandleConnections) {
                IsBackground = true
            };
            t.Start();

            userInputMQ.ReceiveCompleted += UserInputRecieved;
            userInputMQ.BeginReceive();

            Console.ReadKey();
        }

        private static void UserInputRecieved(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mQ = (MessageQueue)sender;
            Message m = mQ.EndReceive(e.AsyncResult);

            try
            {
                switch (Enum.Parse(typeof(TokenRequestType), m.Label))
                {
                    case TokenRequestType.CreateToken:
                        {
                        UserModel originModel = MSMQHelper.GetMessageBody<UserModel>(m);
                        UserModel userModel = new UserModel()
                            {
                                UserID = originModel.UserID, RequestType = originModel.RequestType
                            };
                            Console.WriteLine("UserModel received!");

                            MSMQHelper.SendMessage(beaconInputMQ, "ServersData", "ServersData", beaconResponseMQ);

                            ServersData data = MSMQHelper.GetMessageBody<ServersData>(MSMQHelper.ReceiveMessage(beaconResponseMQ, new TimeSpan(0, 0, 5)));

                            JWTPayload payload = new JWTPayload() { UserID = userModel.UserID, ServersInfo = data };

                            userModel.Token = JWTManager.CreateJWT(JWTManager.CreateClaims<JWTPayload>(payload), 5).RawData;
                            userModel.TokenResponse = TokenResponse.Created;

                            Message userResponse = new Message() {
                                Formatter = new JsonMessageFormatter(),
                                Body = JsonConvert.SerializeObject(userModel),
                                Label = userModel.UserID
                            };

                            MSMQHelper.SendMessage(m.ResponseQueue, userResponse);
                            Console.WriteLine("Token send to {0}", m.ResponseQueue.Path);
                            break;
                        }
                    case TokenRequestType.VerifyToken:
                        {
                            UserModel userModel = MSMQHelper.GetMessageBody<UserModel>(m);

                            if (JWTManager.VerifyToken(userModel.Token))
                            {
                                userModel.TokenResponse = TokenResponse.Valid;
                                userModel.Message = "Token Valid, Connecting to Server!";
                            Console.WriteLine("\n=======TOKEN======");
                                Console.WriteLine(userModel.Token);
                            Console.WriteLine("=======TOKEN======\n");

                            Message userResponse = new Message() {
                                Formatter = new JsonMessageFormatter(),
                                Body = JsonConvert.SerializeObject(userModel),
                                Label = userModel.UserID
                            };
                                MSMQHelper.SendMessage(m.ResponseQueue, userResponse);
                                Console.WriteLine("Token was valid!");
                                Console.WriteLine("Response send to {0}", m.ResponseQueue.Path);
                            }
                            else
                            {
                                userModel.TokenResponse = TokenResponse.Invalid;
                                userModel.Message = "Session Token no longer valid!\n Please login, using credentials.";

                                Message userResponse = new Message() {
                                    Formatter = new JsonMessageFormatter(),
                                    Body = JsonConvert.SerializeObject(userModel),
                                    Label = userModel.UserID
                                };

                                MSMQHelper.SendMessage(m.ResponseQueue, userResponse);
                                Console.WriteLine("Token was invalid!");
                                Console.WriteLine("Response send to {0}", m.ResponseQueue.Path);
                            }
                            break;
                        }
                }
            }
            catch(Exception error)
            {
                Console.WriteLine("Couldn't read message!");
                Console.WriteLine(error);
                Console.WriteLine(error.Message);
            }

            mQ.BeginReceive();
        }

        static void HandleConnections()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, GlobalVariables.TOKENSYSTEM_PORT);
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("Connected!");

                StreamReader stream = new StreamReader(client.GetStream());

                string recievedToken = stream.ReadLine();

                if (JWTManager.VerifyToken(recievedToken))
                {
                    //string response = "Token was valid!";
                    //byte[] data = System.Text.Encoding.ASCII.GetBytes(response);
                    byte[] data = BitConverter.GetBytes((int)HttpStatusCode.OK);
                    client.GetStream().Write(data, 0, data.Length);
                    Console.WriteLine("Valid token response send to {0}", client.Client.RemoteEndPoint.ToString());
                }
                else
                {
                    //string response = "Token was invalid!";
                    //byte[] data = System.Text.Encoding.ASCII.GetBytes(response);
                    byte[] data = BitConverter.GetBytes((int)HttpStatusCode.Unauthorized);
                    client.GetStream().Write(data, 0, data.Length);
                    Console.WriteLine("Invalid token response send to {0}", client.Client.RemoteEndPoint.ToString());
                }
            }
        }
    }
}
