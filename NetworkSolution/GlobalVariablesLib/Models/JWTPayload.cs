using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GlobalVariablesLib {
    public class JWTPayload {
        public ServersData ServersInfo { get; set; }
        public string UserID { get; set; }
    }
}
