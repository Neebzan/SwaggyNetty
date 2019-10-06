using GlobalVariablesLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQL_PlayerData_Translator {
    class Program {
        private static DBConnection DBConnectionMaster;
        private static DBConnection DBConnectionSlave1;
        private static DBConnection DBConnectionSlave2;
        private static MSMQHandler handler;

        private static PlayerDataModel data;

        private static void Main (string [ ] args) {

            SetupDBConnections();
            handler = new MSMQHandler();
            data = new PlayerDataModel();
            handler.NewInputRecieved += OnInputRecieved;
        }

        private static void OnInputRecieved (object sender, InputRecievedEventArgs e) {
            switch (e.RequestType) {
                case PlayerDataRequest.Create:
                    data = DBConnectionMaster.Insert(data);
                    handler.EnqueueProducerQueue(data);
                    break;

                case PlayerDataRequest.Update:
                    data = DBConnectionMaster.Update(data);
                    handler.EnqueueProducerQueue(data);
                    break;

                case PlayerDataRequest.Read:
                    if (data.ReadSlaveNumber == 1) {
                        data = DBConnectionSlave1.Select(data);
                    }
                    else {
                        data = DBConnectionSlave2.Select(data);
                    }
                    handler.EnqueueProducerQueue(data);
                    break;

                default:
                    break;
            }

            if (data.PlayerDataStatus == PlayerDataStatus.Success)
                Console.WriteLine("Success");
            else
                Console.WriteLine("Failure");
        }

        static void SetupDBConnections () {
            DBConnectionMaster = new DBConnection();
            DBConnectionMaster.DatabaseName = "player_data";
            DBConnectionMaster.ServerIP = "178.155.161.248";
            DBConnectionMaster.ServerPort = 3307;
            DBConnectionMaster.Username = "replication_user";
            DBConnectionMaster.Password = "swaggynetty";

            DBConnectionSlave1 = new DBConnection();
            DBConnectionSlave1.DatabaseName = "player_data";
            DBConnectionSlave1.ServerIP = "178.155.161.248";
            DBConnectionSlave1.ServerPort = 3308;
            DBConnectionSlave1.Username = "replication_reader";
            DBConnectionSlave1.Password = "swaggynetty";

            DBConnectionSlave2 = new DBConnection();
            DBConnectionSlave2.DatabaseName = "player_data";
            DBConnectionSlave2.ServerIP = "178.155.161.248";
            DBConnectionSlave2.ServerPort = 3309;
            DBConnectionSlave2.Username = "replication_reader";
            DBConnectionSlave2.Password = "swaggynetty";
        }
    }
}
