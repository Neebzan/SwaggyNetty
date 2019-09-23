using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Helpers;
using Newtonsoft.Json;

namespace Login_Middleware
{
    public class Database_Request_Handler
    {

        private Json_Obj json_obj { get; set; }

        public Database_Request_Handler(string data)
        {

        }

        private Json_Obj DeserializeRequest(string data)
        {
            return (Json_Obj)JsonConvert.DeserializeObject(data);
        }

        public void Get_User(string data)
        {
            
        }

        public void Create_User()
        {

        }

        public void Update_User()
        {

        }

        public void Delete_User()
        {

        }

    }
}
