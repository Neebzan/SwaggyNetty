using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Login_Middleware
{
    class Json_Obj
    {
        public string UserID { get; set; }
        public string PswdHash { get; set; }
        public string Token { get; set; }

        /// <summary>
        /// Debug Message
        /// </summary>
        public string Message { get; set; }
        public Socket Socket { get; set; }
        public enum RequestStatus { Success, AlreadyExists, DoesNotExist, ConnectionError }
        public RequestStatus Status { get; set; }
        public enum RequestTypes { Get_User, Create_User, Update_User, Delete_User, Response };
        public RequestTypes RequestType { get; set; }


    }
}
