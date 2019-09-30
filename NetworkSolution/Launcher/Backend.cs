using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Launcher {
    public static class Backend {
<<<<<<< HEAD
=======
        private static string middlewareIP = "10.131.69.129";
        private static int middlewarePort = 13010;
        public static UserModel loggedUser { get; private set; }
>>>>>>> parent of 7e1d543... Updated UI

        private static string middlewareIP = "127.0.0.1";
        private static int middlewarePort = 420;
        

        public static async Task SendLoginCredentials (string username, SecureString password) {
            TcpClient client = new TcpClient(middlewareIP, middlewarePort);

            string unsecurePassword = ConvertToUnsecureString(password);
            string hashedPassword = GetPasswordHash(unsecurePassword);

            await client.ConnectAsync(middlewareIP, middlewarePort);
        }

        private static string GetPasswordHash (string password) {

            using (HMACSHA512 t = new HMACSHA512(Encoding.UTF8.GetBytes(password))) {
                byte [ ] hash;
                hash = t.ComputeHash(Encoding.UTF8.GetBytes(password));
                password = BitConverter.ToString(hash).Replace("-", "");
            }

            return password;
        }
        private static string ConvertToUnsecureString (SecureString securePassword) {
            if (securePassword == null) {
                return string.Empty;
            }

            IntPtr unmanagedString = IntPtr.Zero;
            try {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
