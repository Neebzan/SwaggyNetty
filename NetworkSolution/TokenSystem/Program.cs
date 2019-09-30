using GlobalVariablesLib;
using JWTlib;
using MSMQHelperUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TokenSystem
{
    class Program
    {
        
        static void Main(string[] args)
        {
            TokenSystem tokenSystem;
            tokenSystem = new TokenSystem();
        }        
    }
}
