using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpHelper {
    public static class MessageFormatter {

        /// <summary>
        /// Takes in a string, converts to byte array, adds a integer header before message, which value is the length of the byte message. Then returns the full package
        /// </summary>
        /// <param name="_message"></param>
        /// <returns></returns>
        public static byte [ ] MessageBytes (string _message) {
            byte [ ] packageData = System.Text.Encoding.ASCII.GetBytes(_message);

            byte [ ] totalPackage = AddSizeHeaderToPackage(packageData);

            return totalPackage;
        }

        /// <summary>
        /// Serializes to JSON, converts to byte array, and adds a header to the package, consisting of an integer value of the byte length of the orignial message
        /// </summary>
        /// <typeparam name="T">The type of object to JSON serialize</typeparam>
        /// <param name="obj">The object to serialize as a message</param>
        /// <returns></returns>
        public static byte [ ] MessageBytes<T> (T obj) {
            string packageJson = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            string msg = packageJson;
            //Convert to JSON
            byte [ ] packageData = System.Text.Encoding.ASCII.GetBytes(msg);

            byte [ ] totalPackage = AddSizeHeaderToPackage(packageData);

            return totalPackage;
        }


        private static byte [ ] AddSizeHeaderToPackage (byte [ ] _package) {
            //Create a uint containing the length of the package, and encode to byte array
            int pckLen = _package.Length;
            byte [ ] packageHeader = BitConverter.GetBytes(pckLen);
            byte [ ] totalPackage = new byte [ packageHeader.Length + _package.Length ];
            //Merge byte arrays
            System.Buffer.BlockCopy(packageHeader, 0, totalPackage, 0, packageHeader.Length);
            System.Buffer.BlockCopy(_package, 0, totalPackage, packageHeader.Length, _package.Length);

            return totalPackage;
        }

    }
}
