using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TokenSystem
{
    public class Payload
    {
        public string UserID { get; set; }
        public string ClientSocket { get; set; }
        public string ServerID { get; set; }
        public string ServerSocket { get; set; }
        public string ChatSocket { get; set; }
    }
}
