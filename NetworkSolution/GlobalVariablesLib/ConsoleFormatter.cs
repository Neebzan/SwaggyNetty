using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalVariablesLib {
    public static class ConsoleFormatter {

        public static void WriteLineWithTimestamp(string msg) {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo) + "  -  " + msg);
        }
    }
}
