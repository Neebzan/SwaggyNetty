using GlobalVariablesLib;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQL_PlayerData_Translator
{
    public class DBConnection
    {
        private string databaseName = string.Empty;
        public string DatabaseName
        {
            get { return databaseName; }
            set { databaseName = value; }
        }

        private string serverIP;
        public string ServerIP
        {
            get { return serverIP; }
            set { serverIP = value; }
        }

        private int serverPort;
        public int ServerPort
        {
            get { return serverPort; }
            set { serverPort = value; }
        }

        private string username;
        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public string Password { get; set; }

        public MySqlConnection CreateConnection()
        {
            string connstring = string.Format("datasource={0}; port={1}; database={2}; username={3}; password={4}", ServerIP, ServerPort, DatabaseName, Username, Password);
            MySqlConnection connection = new MySqlConnection(connstring);
            return connection;
        }


        public PlayerDataModel Select(PlayerDataModel data)
        {
            using (MySqlConnection connection = CreateConnection())
            {
                try
                {
                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "SELECT user_id, position_x, position_y, online FROM players WHERE user_id = @user_id";
                    command.Parameters.AddWithValue("@user_id", data.UserID);

                    MySqlDataReader reader = command.ExecuteReader();

                    string user_id = "";
                    float position_x = 0;
                    float position_y = 0;
                    bool online = false;
                    DateTimeOffset createdAt = DateTime.Now;

                    while (reader.Read())
                    {
                        user_id = reader["user_id"].ToString();
                        position_x = float.Parse((reader["position_x"].ToString()));
                        position_y = float.Parse((reader["position_y"].ToString()));
                        online = Convert.ToBoolean((reader["online"]));
                    }
                    if (!string.IsNullOrEmpty(user_id))
                    {
                        data.PositionX = position_x;
                        data.PositionY = position_y;
                        data.Online = online;
                        data.PlayerDataStatus = PlayerDataStatus.Success;
                        return data;
                    }

                    else
                    {
                        ConsoleFormatter.WriteLineWithTimestamp("User with ID " + data.UserID + " does not exist, or couldn't be found");
                        data.PlayerDataStatus = PlayerDataStatus.DoesNotExist;
                        return data;
                    }
                }
                catch (Exception e)
                {
                    ConsoleFormatter.WriteLineWithTimestamp("ERROR: " + e.Message);
                    data.PlayerDataStatus = PlayerDataStatus.ConnectionFailed;
                    return data;
                }
            }
        }

        public PlayerDataModel Insert(PlayerDataModel data)
        {
            using (MySqlConnection connection = CreateConnection())
            {
                try
                {
                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "INSERT INTO players (user_id, position_x, position_y, online) VALUES (@user_id, @position_x, @position_y, @online)";


                    command.Parameters.AddWithValue("@user_id", data.UserID);
                    command.Parameters.AddWithValue("@position_x", data.PositionX);
                    command.Parameters.AddWithValue("@position_y", data.PositionY);
                    command.Parameters.AddWithValue("@online", data.Online);


                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        ConsoleFormatter.WriteLineWithTimestamp("User " + data.UserID + " inserted");
                        data.PlayerDataStatus = PlayerDataStatus.Success;
                        return data;
                    }
                    catch (Exception e)
                    {
                        ConsoleFormatter.WriteLineWithTimestamp("ERROR: " + e.Message);
                        data.PlayerDataStatus = PlayerDataStatus.AlreadyExists;
                        return data;
                    }
                }
                catch (Exception e)
                {
                    ConsoleFormatter.WriteLineWithTimestamp("ERROR: " + e.Message);
                    data.PlayerDataStatus = PlayerDataStatus.ConnectionFailed;
                    return data;
                }
            }
        }

        public PlayerDataModel Update(PlayerDataModel data)
        {
            using (MySqlConnection connection = CreateConnection())
            {
                try
                {
                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "UPDATE players " +
                        "SET user_id = @user_id," +
                        "position_x = @position_x," +
                        "position_y = @position_y," +
                        "online = @online " +
                        "WHERE user_id = @user_id";


                    command.Parameters.AddWithValue("@user_id", data.UserID);
                    command.Parameters.AddWithValue("@position_x", data.PositionX);
                    command.Parameters.AddWithValue("@position_y", data.PositionY);
                    command.Parameters.AddWithValue("@online", data.Online);


                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        ConsoleFormatter.WriteLineWithTimestamp("User " + data.UserID + " updated");
                        data.PlayerDataStatus = PlayerDataStatus.Success;
                        return data;
                    }
                    catch (Exception e)
                    {
                        ConsoleFormatter.WriteLineWithTimestamp("ERROR: " + e.Message);
                        data.PlayerDataStatus = PlayerDataStatus.DoesNotExist;
                        return data;
                    }
                }
                catch (Exception e)
                {
                    ConsoleFormatter.WriteLineWithTimestamp("ERROR: " + e.Message);
                    data.PlayerDataStatus = PlayerDataStatus.ConnectionFailed;
                    return data;
                }
            }
        }
    }
}
