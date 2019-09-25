using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Login_Middleware;
using Newtonsoft.Json;

namespace LoginClient
{
    class Login_Client_Main
    {
        private static TcpClient client = new TcpClient();
        private static Socket socket;
        private static NetworkStream networkStream;

        private static bool waitingForResponse = false;
        public static bool Connected
        {
            get
            {
                try
                {
                    if (socket != null && socket.Connected)
                    {
                        if (socket.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];

                            if (socket.Receive(buff, SocketFlags.Peek) == 0)
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

        public static void Main(string[] args)
        {
            Console.WriteLine("/// --- | Client Started | --- ///");

            Console.WriteLine("Attempting to Connect");

            // Connects the Client to remote Host
            client.Connect("127.0.0.1", 13000);

            // Sets a Socket for connection check
            socket = client.Client;

            // Gets the stream from TCP Client
            networkStream = client.GetStream();

            byte[] byteArr = new byte[256];

            Task.Factory.StartNew(WriteMessages);
            Task.Factory.StartNew(ListenForMessages);
            while (Connected)
            {

            }
            Thread.Sleep(1500);
            Console.WriteLine("\nConnection Closed...");
            Console.ReadLine();
        }

        private static void WriteMessages()
        {
            while (Connected && !waitingForResponse)
            {
                try
                {
                    Console.WriteLine("Enter Username:");
                    string userid = Console.ReadLine();
                    Console.WriteLine("Enter Password");
                    string userpwsrd = Console.ReadLine();

                    Json_Obj MessageRequest = new Json_Obj()
                    {
                        UserID = userid,
                        PswdHash = userpwsrd,
                        RequestType = Json_Obj.RequestTypes.Get_User
                    };

                    string messageData = JsonConvert.SerializeObject(MessageRequest);

                    // Get bytes from data (string)
                    byte[] msg = Encoding.ASCII.GetBytes(messageData);
                    //sends message to client
                    networkStream.Write(msg, 0, msg.Length);

                    waitingForResponse = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("_________________________________________________________________________________");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("_________________________________________________________________________________");
                }
            }
        }

        private static void ListenForMessages()
        {
            try
            {
                while (Connected)
                {
                    int i = 0;

                    Byte[] bytes = new Byte[256];
                    String data = null;

                    while ((i = networkStream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine(data);
                        try
                        {
                            Json_Obj json_Obj = JsonConvert.DeserializeObject<Json_Obj>(data);

                            switch (json_Obj.RequestType)
                            {
                                case Json_Obj.RequestTypes.Get_User:
                                    break;
                                case Json_Obj.RequestTypes.Create_User:
                                    break;
                                case Json_Obj.RequestTypes.Update_User:
                                    break;
                                case Json_Obj.RequestTypes.Delete_User:
                                    break;
                                case Json_Obj.RequestTypes.Response:
                                    Console.WriteLine($"Message Type: {json_Obj.RequestType.ToString()} \nMessage: {json_Obj.Message}\nStatus: {json_Obj.Status.ToString()}");
                                    waitingForResponse = false;
                                    break;
                                case Json_Obj.RequestTypes.Error:
                                    Console.WriteLine($"Message Type: {json_Obj.RequestType.ToString()} \nError: {json_Obj.Message}");
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("_________________________________________________________________________________");
                Console.WriteLine(e.Message);
                Console.WriteLine("_________________________________________________________________________________");

            }
        }
    }
}
