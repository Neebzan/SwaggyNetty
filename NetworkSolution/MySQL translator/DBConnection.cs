using MySql.Data.MySqlClient;
using MySql.Data;
using System;
using System.Collections.Generic;
using GlobalVariablesLib;

namespace MySQL_translator {

    //Some code taken from https://stackoverflow.com/questions/21618015/how-to-connect-to-mysql-database
    //By users Ocph23 & Moffen

    public class DBConnection {
        private string databaseName = string.Empty;
        public string DatabaseName {
            get { return databaseName; }
            set { databaseName = value; }
        }

        private string serverIP;
        public string ServerIP {
            get { return serverIP; }
            set { serverIP = value; }
        }

        private int serverPort;
        public int ServerPort {
            get { return serverPort; }
            set { serverPort = value; }
        }

        private string username;
        public string Username {
            get { return username; }
            set { username = value; }
        }


        public string Password { get; set; }

        private DBConnection () {

        }

        private static DBConnection _instance = null;
        public static DBConnection Instance () {
            if (_instance == null)
                _instance = new DBConnection();
            return _instance;
        }

        public MySqlConnection CreateConnection () {
            string connstring = string.Format("datasource={0}; port={1}; database={2}; username={3}; password={4}", ServerIP, ServerPort, DatabaseName, Username, Password);
            MySqlConnection connection = new MySqlConnection(connstring);
            return connection;
        }

        /// <summary>
        /// SELECT user_id, password_hash FROM users WHERE user_id = _user_id
        /// </summary>
        /// <param name="_user_id"></param>
        public UserModel Select (UserModel _user) {
            using (MySqlConnection connection = CreateConnection()) {
                try {
                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "SELECT user_id, password_hash, created_at FROM users WHERE user_id = @user_id";
                    command.Parameters.AddWithValue("@user_id", _user.UserID);

                    MySqlDataReader reader = command.ExecuteReader();

                    string userID = "";
                    string userPass = "";
                    DateTimeOffset createdAt = DateTime.Now;

                    while (reader.Read()) {
                        userID = reader [ "user_id" ].ToString();
                        userPass = reader [ "password_hash" ].ToString();
                        createdAt = DateTimeOffset.Parse(reader [ "created_at" ].ToString());
                    }
                    if (!string.IsNullOrEmpty(userID)) {
                        ConsoleFormatter.WriteLineWithTimestamp("User with ID " + _user.UserID + " returned");
                        _user.Status = RequestStatus.Success;
                        _user.PswdHash = userPass;
                        _user.CreatedAt = createdAt;
                        return _user;
                    }
                    else {
                        ConsoleFormatter.WriteLineWithTimestamp("User with ID " + _user.UserID + " does not exist, or couldn't be found");
                        _user.Status = RequestStatus.DoesNotExist;
                        return _user;
                    }
                }
                catch (Exception e) {
                    ConsoleFormatter.WriteLineWithTimestamp("ERROR: " + e.Message);
                    _user.Status = RequestStatus.ConnectionError;
                    return _user;
                }
            }
        }

        /// <summary>
        /// Inserts user into database
        /// </summary>
        /// <param name="_user"></param>
        /// <returns></returns>
        public UserModel Insert (UserModel _user) {
            using (MySqlConnection connection = CreateConnection()) {
                try {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "INSERT INTO users (user_id, password_hash) VALUES (@user_id, @password_hash)";

                    command.Parameters.AddWithValue("@user_id", _user.UserID);
                    command.Parameters.AddWithValue("@password_hash", _user.PswdHash);

                    try {
                        int rowsAffected = command.ExecuteNonQuery();
                        ConsoleFormatter.WriteLineWithTimestamp("User " + _user.UserID + " inserted");
                        _user.Status = RequestStatus.Success;
                        return _user;
                    }
                    catch (Exception e) {
                        ConsoleFormatter.WriteLineWithTimestamp("ERROR: " + e.Message);
                        _user.Status = RequestStatus.AlreadyExists;
                        return _user;
                    }
                }
                catch (Exception e) {
                    ConsoleFormatter.WriteLineWithTimestamp("ERROR: " + e.Message);
                    _user.Status = RequestStatus.ConnectionError;
                    return _user;
                }
            }
        }

    }
}
