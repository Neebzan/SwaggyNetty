using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TokenSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionModel testModel = new ConnectionModel { IP = "an IP", Port = 123 };

            var e = JWTManager.CreateJWT(JWTManager.CreateClaims<ConnectionModel>("My ID", testModel), 5);

            var tttt = JWTManager.GetModelFromToken<ConnectionModel>(e);
            
        }
    }
}
