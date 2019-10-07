using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GlobalVariablesLib
{
    //[Serializable]
    public class UserModel
    {
        public string UserID { get; set; }
        public string PswdHash { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string RemoteEndPoint { get; set; }

        public string Token { get; set; }
        /// <summary>
        /// Debug Message
        /// </summary>
        public string Message { get; set; }

        public RequestStatus Status { get; set; }
        public RequestType RequestType { get; set; }

        public TokenResponse TokenResponse { get; set; }
    }
}
