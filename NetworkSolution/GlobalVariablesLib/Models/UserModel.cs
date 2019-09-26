using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalVariablesLib
{
    [Serializable]
    public class UserModel
    {
        public string UserID { get; set; }
        public string PswdHash { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public string Token { get; set; }
        /// <summary>
        /// Debug Message
        /// </summary>
        public string Message { get; set; }

        public RequestStatus Status { get; set; }
        public RequestTypes RequestType { get; set; }
    }
}
