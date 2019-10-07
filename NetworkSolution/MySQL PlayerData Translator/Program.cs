using GlobalVariablesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySQL_PlayerData_Translator
{
    class Program
    {
        private static DBConnection dbConnectionMaster;
        private static DBConnection dbConnectionSlave1;
        private static DBConnection dbConnectionSlave2;
        private static MSMQHandler handler;

        private static void Main(string[] args)
        {
            SetupDBConnections();
            handler = new MSMQHandler();
            handler.NewInputRecieved += OnInputRecieved;


            Console.ReadKey();
        }

        private static void OnInputRecieved(object sender, InputRecievedEventArgs e)
        {
            Console.WriteLine("Thread: " + Task.CurrentId + " executing OnInputRecieved");
            switch (e.RequestType)
            {
                case PlayerDataRequest.Create:
                    
                    handler.EnqueueProducerQueue(dbConnectionMaster.Insert(e.Data));
                    break;

                case PlayerDataRequest.Update:
                    
                    handler.EnqueueProducerQueue(dbConnectionMaster.Update(e.Data));
                    break;

                case PlayerDataRequest.Read:
                    if (e.Data.ReadSlaveNumber == 1)
                    {                       
                        handler.EnqueueProducerQueue(dbConnectionSlave1.Select(e.Data));
                    }
                    else
                    {                      
                        handler.EnqueueProducerQueue(dbConnectionSlave2.Select(e.Data));
                    }
                    break;

                default:
                    break;
            }

            if (e.Data.PlayerDataStatus == PlayerDataStatus.Success)
                Console.WriteLine("Success");
            else
                Console.WriteLine("Failure");
        }

        static void SetupDBConnections()
        {
            dbConnectionMaster = new DBConnection();
            dbConnectionSlave1 = new DBConnection();
            dbConnectionSlave2 = new DBConnection();

            dbConnectionMaster.Username = GlobalVariablesLib.GlobalVariables.MYSQL_PLAYER_DB_MASTER_USERNAME;
            dbConnectionMaster.Password = GlobalVariablesLib.GlobalVariables.MYSQL_PLAYER_DB_MASTER_PASSWORD;
            dbConnectionSlave1.Username = dbConnectionSlave2.Username = GlobalVariablesLib.GlobalVariables.MYSQL_PLAYER_DB_SLAVE_USERNAME;
            dbConnectionSlave1.Password = dbConnectionSlave2.Password = GlobalVariablesLib.GlobalVariables.MYSQL_PLAYER_DB_SLAVE_PASSWORD;

            dbConnectionMaster.DatabaseName = dbConnectionSlave1.DatabaseName = dbConnectionSlave2.DatabaseName = GlobalVariablesLib.GlobalVariables.MYSQL_PLAYER_DB_DATABASENAME;
            dbConnectionMaster.ServerIP = dbConnectionSlave1.ServerIP = dbConnectionSlave2.ServerIP = GlobalVariablesLib.GlobalVariables.MYSQL_PLAYER_DB_IP;
            dbConnectionMaster.ServerPort = GlobalVariablesLib.GlobalVariables.MYSQL_PLAYER_DB_MASTER_PORT;
            dbConnectionSlave1.ServerPort = GlobalVariablesLib.GlobalVariables.MYSQL_PLAYER_DB_SLAVE1_PORT;
            dbConnectionSlave2.ServerPort = GlobalVariablesLib.GlobalVariables.MYSQL_PLAYER_DB_SLAVE2_PORT;

        }
    }
}
