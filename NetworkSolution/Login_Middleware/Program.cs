using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace Login_Middleware
{
    class Program
    {
        static Queue<Database_Request_Handler> databaseRequests = new Queue<Database_Request_Handler>();

        static void Main(string[] args)
        {
            
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

            DynamicJsonObject db_Obj = Json.Decode(logOnRequest.Request_Get(data));

            if (data == db_Obj)
            {
                Request_Token(Json.Encode(data));
            }

        }

        
    }
}
