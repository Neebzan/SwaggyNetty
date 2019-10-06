using GlobalVariablesLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQL_PlayerData_Translator {
    class Program {
        static DBConnection DBConnectionMaster;
        static DBConnection DBConnectionSlave1;
        static DBConnection DBConnectionSlave2;

        static void Main (string [ ] args) {

            SetupDBConnections();

            //var data = DBConnectionSlave1.Select(new PlayerDataModel() { UserID = "Neebz", PlayerDataRequest = PlayerDataRequest.Read });
            //if (data.PlayerDataStatus == PlayerDataStatus.Success) {
            //    Console.WriteLine("Success");
            //}
            //else {
            //    Console.WriteLine("Failure");
            //}
            //Console.ReadKey();


            var data2 = DBConnectionMaster.Insert(new PlayerDataModel() { UserID = "Swaggerdude4", PositionX = 1, PositionY = 1, Online = false });
            if (data2.PlayerDataStatus == PlayerDataStatus.Success) {
                Console.WriteLine("Success");
            }
            else {
                Console.WriteLine("Failure");
            }
            Console.ReadKey();

            //var data2 = DBConnectionMaster.Update(new PlayerDataModel() { UserID = "Neebz", PositionX = 1, PositionY = 1, Online = false });
            //if (data2.PlayerDataStatus == PlayerDataStatus.Success) {
            //    Console.WriteLine("Success");
            //}
            //else {
            //    Console.WriteLine("Failure");
            //}
            //Console.ReadKey();
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
