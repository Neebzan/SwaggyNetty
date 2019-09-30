using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchManagerServer {
    class Program {
        static void Main (string [ ] args) {
            PatchmanagerServer server = new PatchmanagerServer();
            Console.ReadKey();
        }
    }
}
