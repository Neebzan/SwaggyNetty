using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GlobalVariablesLib
{
    public class UserSession
    {
        public TcpClient Client { get; set; }
        public bool InGame { get; set; } = false;
        public string UserID { get; set; }
        public SessionRequest Request { get; set; }
    }
}
