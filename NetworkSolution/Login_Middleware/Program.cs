using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace Login_Middleware
{
    class Program
    {
        static int port = 0;
        static private IPAddress IP = IPAddress.Any;

        static Queue<Database_Request_Handler> databaseRequests = new Queue<Database_Request_Handler>();
        static ConcurrentQueue<Middleware_Client> users = new ConcurrentQueue<Middleware_Client>();

        static public TcpListener serverListener = new TcpListener(IP,port);
        static void Main(string[] args)
        {
            
        }

        public static void ListenForClients()
        {
            serverListener.Start();

            while (true)
            {
                TcpClient c = serverListener.AcceptTcpClient();
                users.Enqueue(new Middleware_Client(c));
            }
        }

        static void TCP_Request_Handler(string data)
        {
            Database_Request_Handler dbh = databaseRequests.Dequeue();
            dynamic dataObj = Json.Decode(data);


            switch (dataObj.RequestType)
            {
                case "Login":
                    Login(dataObj,dbh);
                    break;
                case "Logout":
                    break;
                case "Create":
                    break;
                case "Delete":
                    break;
                default:
                    break;
            }
        }

        static void Request_Token(string data)
        {

        }

        static string Decrypter(string data)
        {
            return data;
        }

        static string Encrypter(string data)
        {
            return data;
        }


        static internal void Login(DynamicJsonObject data, Database_Request_Handler logOnRequest)
        {

            dynamic db_Obj = Json.Decode(logOnRequest.Request_Get(data));

            if(db_Obj.User_ID == data.User_ID)

            if (data == db_Obj)
            {
                Request_Token(Json.Encode(data));
            }

        }

        
    }
}
