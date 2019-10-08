using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TcpHelper;
using GlobalVariablesLib;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Threading;

namespace StressTester {
    class Program {
        static List<TcpClient> tcpClients = new List<TcpClient>();

        static IPAddress ip = IPAddress.Parse("127.0.0.1");
        static int port = 13010;
        static bool alive = true;

        /// <summary>
        /// Connection check property. Returns a true if the TcpClient is currently connected, else returns false.
        /// Credit: Robert Conan McMillan
        /// </summary>
        public static bool Connected(TcpClient client) {
            try {
                if (client.Client != null && client.Client.Connected) {
                    if (client.Client.Poll(0, SelectMode.SelectRead)) {
                        byte[] buff = new byte[1];
                        if (client.Client.Receive(buff, SocketFlags.Peek) == 0) {
                            return false;
                        }
                        return true;
                    }
                    return true;
                }
                return false;
            } catch {
                return false;
            }
        }

        static void Main(string[] args) {
            Console.WriteLine("Enter Number to Spam");
            int spam = Convert.ToInt32(Console.ReadLine());
            for (int i = 0; i < spam; i++) {
                tcpClients.Add(new TcpClient());
            }

            foreach (TcpClient client in tcpClients) {
                Console.WriteLine("New Client Spammer Startet.");
                Task.Factory.StartNew(() => Spammer(client));
            }

            while (alive) {
                Thread.Sleep(5);
            }
            alive = false;
        }

        /// <summary>
        /// Password has function from Esben Juhl Dalsgaard
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private static string GetPasswordHash(string password) {
            using (HMACSHA512 t = new HMACSHA512(Encoding.UTF8.GetBytes(password))) {
                byte[] hash;
                hash = t.ComputeHash(Encoding.UTF8.GetBytes(password));
                password = BitConverter.ToString(hash).Replace("-", "");
            }
            return password;
        }

        static void Spammer(TcpClient client) {
            while (alive) {
                Thread.Sleep(10);
                try {
                    UserModel userModel;
                    byte[] byteArr;

                    if (Connected(client)) {
                        userModel = new UserModel() {
                            UserID = client.Client.RemoteEndPoint.ToString(), RequestType = RequestType.Get_User,
                            PswdHash = GetPasswordHash(client.Client.RemoteEndPoint.ToString())
                        };

                        byteArr = MessageFormatter.MessageBytes(JsonConvert.SerializeObject(userModel));

                        client.GetStream().Write(byteArr, 0, byteArr.Length);

                        Console.WriteLine($"Spammer Sent Message {client.Client.RemoteEndPoint.ToString()}");

                    } else {
                        client.Close();
                        client = new TcpClient();
                        client.Connect(ip, port);
                    }

                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                }

            }

        }
    }
}
