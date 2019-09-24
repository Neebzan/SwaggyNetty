using MSMQHelperUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySQL_translator
{
    class Program
    {
        static MessageQueueHandler mQHandler;

        static void Main (string [ ] args) {
            mQHandler = new MessageQueueHandler();
            mQHandler.NewInputRecieved += InputRecieved;

            SetupDBConnection();

            ConsoleInputLoop();
        }

        static void SetupDBConnection () {
            DBConnection.Instance().DatabaseName = "authentication_data";
            DBConnection.Instance().ServerIP = "178.155.161.248";
            DBConnection.Instance().ServerPort = 3306;
            DBConnection.Instance().Username = "netty";
            DBConnection.Instance().Password = "swaggynetty";
        }

        private static void InputRecieved (object sender, InputRecievedEventArgs e) {
            User newUser = null;

            switch (e.requestType) {
                case RequestTypes.Get_User:
                    newUser = DBConnection.Instance().Select(e.User);
                    mQHandler.PushProducerQueue(newUser);
                    break;

                case RequestTypes.Create_User:
                    newUser = DBConnection.Instance().Insert(e.User);
                    mQHandler.PushProducerQueue(newUser);
                    break;

                default:
                    Console.WriteLine("Inputs recieved, but command was unknown");
                    break;
            }
        }

        private static void ConsoleInputLoop () {
            User newUser = null;

            while (true) {

                Console.WriteLine(" R: SELECT FROM users");
                Console.WriteLine(" S: SELECT * FROM users");
                Console.WriteLine(" I: INSERT INTO users\n");

                var key = Console.ReadKey().Key;

                Console.Clear();

                string user_id = "";
                string password_hash = "";

                switch (key) {
                    case ConsoleKey.R:
                        Console.Write("user_id: ");
                        user_id = Console.ReadLine();

                        newUser = DBConnection.Instance().Select(new User() { UserID = user_id });

                        if (newUser.RequestStatus == RequestStatus.Success) {
                            Console.WriteLine("UserID: {0}, PswdHash: {1}, CreatedAt: {2}", newUser.UserID, newUser.PswdHash, newUser.CreatedAt);
                        }
                        break;
                    case ConsoleKey.S:
                        DBConnection.Instance().ConsoleSelect();

                        break;
                    case ConsoleKey.I:
                        Console.Write("user_id: ");
                        user_id = Console.ReadLine();

                        Console.Write("password_hash: ");
                        password_hash = Console.ReadLine();

                        newUser = DBConnection.Instance().Insert(new User() { UserID = user_id, PswdHash = password_hash });
                        
                        if (newUser.RequestStatus == RequestStatus.Success) {
                            Console.WriteLine("UserID: {0}, PswdHash: {1}", newUser.UserID, newUser.PswdHash);
                        }
                        
                        break;
                    case ConsoleKey.Escape:
                        Environment.Exit(1);
                        break;
                    default:
                        break;
                }
                Console.WriteLine("\n");
            }
        }

    }
}
