using MSMQHelperUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GlobalVariablesLib;

namespace MySQL_translator {
    class Program {
        static MessageQueueHandler mQHandler;

        static void Main (string [ ] args) {
            mQHandler = new MessageQueueHandler();
            mQHandler.NewInputRecieved += InputRecieved;

            SetupDBConnection();
            while (true) {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                    break;
            }
        }

        static void SetupDBConnection () {
            DBConnection.Instance().DatabaseName = GlobalVariablesLib.GlobalVariables.MYSQL_LOGIN_DB_DATABASENAME;
            DBConnection.Instance().ServerIP = GlobalVariablesLib.GlobalVariables.MYSQL_LOGIN_DB_IP;
            DBConnection.Instance().ServerPort = GlobalVariablesLib.GlobalVariables.MYSQL_LOGIN_DB_PORT;
            DBConnection.Instance().Username = GlobalVariablesLib.GlobalVariables.MYSQL_LOGIN_DB_USERNAME;
            DBConnection.Instance().Password = GlobalVariablesLib.GlobalVariables.MYSQL_LOGIN_DB_PASSWORD;
        }

        private static void InputRecieved (object sender, InputRecievedEventArgs e) {
            switch (e.RequestType) {
                case RequestType.Get_User:
                    mQHandler.EnqueueProducerQueue(DBConnection.Instance().Select(e.User));
                    break;

                case RequestType.Create_User:
                    mQHandler.EnqueueProducerQueue(DBConnection.Instance().Insert(e.User));
                    break;

                default:
                    ConsoleFormatter.WriteLineWithTimestamp("Inputs recieved, but command was unknown");
                    break;
            }
        }
    }
}

