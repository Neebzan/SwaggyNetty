using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatchManagerClient {
    class Program {
        static void Main (string [ ] args) {
            PatchmanagerClient client = new PatchmanagerClient();
            Console.ReadKey();
        }
    }
}
