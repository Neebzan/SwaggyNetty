using GlobalVariablesLib;
using JWTlib;
using MSMQHelperUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpHelper;

namespace TokenSystem
{
    public class TokenSystem
    {
        MessageQueue userInputMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.TOKEN_INPUT_QUEUE_NAME);
        MessageQueue beaconInputMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.BEACON_INPUT_QUEUE_NAME);
        MessageQueue beaconResponseMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.BEACON_RESPONSE_QUEUE_NAME);
        string key = "Pleadssssssssssssssssssssssssssssssssssssssssh";   //Overvej at gøre brug af asymmetric keys og gem nøglen lokalt på serveren     

        List<TcpClient> connectedClients = new List<TcpClient>();

        public TokenSystem()
        {
            Thread t = new Thread(HandleConnections);
            t.IsBackground = true;
            t.Start();

            userInputMQ.ReceiveCompleted += UserInputRecieved;
            userInputMQ.BeginReceive();

            Console.ReadKey();
        }

        private void UserInputRecieved(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mQ = (MessageQueue)sender;
            Message m = mQ.EndReceive(e.AsyncResult);

            try
            {
                switch (Enum.Parse(typeof(TokenRequestType), m.Label))
                {
                    case TokenRequestType.CreateToken:
                        {
                            UserModel userModel = MSMQHelper.GetMessageBody<UserModel>(m);
                            Console.WriteLine("UserModel received!");

                            MSMQHelper.SendMessage(beaconInputMQ, "ServersData", "ServersData", beaconResponseMQ);

                            ServersData data = MSMQHelper.GetMessageBody<ServersData>(MSMQHelper.ReceiveMessage(beaconResponseMQ, new TimeSpan(0, 0, 5)));
                            JWTPayload payload = new JWTPayload() { User = userModel, ServersInfo = data };
                            JwtSecurityToken token = JWTManager.CreateJWT(JWTManager.CreateClaims<JWTPayload>(payload), 5, key);

                            MSMQHelper.SendMessage(m.ResponseQueue, token.RawData);
                            Console.WriteLine("Token send to {0}", m.ResponseQueue.Path);
                            break;
                        }
                    case TokenRequestType.VerifyToken:
                        {
                            if (JWTManager.VerifyToken(MSMQHelper.GetMessageBody<string>(m), key))
                            {
                                MSMQHelper.SendMessage<TokenResponse>(m.ResponseQueue, TokenResponse.Valid);
                                Console.WriteLine("Token was valid!");
                                Console.WriteLine("Response send to {0}", m.ResponseQueue.Path);
                            }
                            else
                            {
                                MSMQHelper.SendMessage<TokenResponse>(m.ResponseQueue, TokenResponse.Invalid);
                                Console.WriteLine("Token was invalid!");
                                Console.WriteLine("Response send to {0}", m.ResponseQueue.Path);
                            }
                            break;
                        }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine("Couldn't read message!");
                Console.WriteLine(error.Message);
            }

            mQ.BeginReceive();
        }

        private void HandleTcpRequest(TcpClient client)
        {
            while (MessageFormatter.Connected(client))
            {
                if (client.GetStream().DataAvailable)
                {
                    string recievedToken = MessageFormatter.ReadMessage(client.GetStream());

                    if (JWTManager.VerifyToken(recievedToken, key))
                    {
                        //string response = "Token was valid!";
                        //byte[] data = System.Text.Encoding.ASCII.GetBytes(response);
                        byte[] data = MessageFormatter.MessageBytes(HttpStatusCode.OK.ToString());
                        client.GetStream().Write(data, 0, data.Length);
                        Console.WriteLine("Valid token response send to {0}", client.Client.RemoteEndPoint.ToString());
                    }
                    else
                    {
                        //string response = "Token was invalid!";
                        //byte[] data = System.Text.Encoding.ASCII.GetBytes(response);
                        byte[] data = MessageFormatter.MessageBytes(HttpStatusCode.Unauthorized.ToString());
                        client.GetStream().Write(data, 0, data.Length);
                        Console.WriteLine("Invalid token response send to {0}", client.Client.RemoteEndPoint.ToString());
                    }
                }
            }

            lock (connectedClients)
                connectedClients.Remove(client);

            Console.WriteLine("{0} disconnected!", client.Client.RemoteEndPoint.ToString());
            Console.WriteLine("Currently {0} other connected clients!", connectedClients.Count);
        }

        private void HandleConnections()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, GlobalVariables.TOKENSYSTEM_PORT);
            listener.Start();
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                Console.WriteLine("Connected!");

                lock (connectedClients)
                {
                    connectedClients.Add(client);
                    Thread t = new Thread(()=>HandleTcpRequest(client));
                    t.IsBackground = true;
                    t.Start();
                }

            }
        }
    }
}
