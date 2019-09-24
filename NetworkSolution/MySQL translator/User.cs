using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQL_translator
{
    public enum RequestTypes { Get_User, Create_User, Update_User, Delete_User, Response };

    public enum RequestStatus { Success, AlreadyExists, DoesNotExist, ConnectionError}

    [Serializable]
    public class User
    {
        public RequestStatus RequestStatus { get; set; }
        public RequestTypes RequestType { get; set; }
        public string UserID { get; set; }
        public string PswdHash { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
