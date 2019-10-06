using GlobalVariablesLib;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQL_PlayerDB_Translator {
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
        public void ConsoleSelect (string _user_id) {
            using (MySqlConnection connection = CreateConnection()) {
                try {
                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "SELECT user_id, password_hash FROM users WHERE user_id = @user_id";
                    command.Parameters.AddWithValue("@user_id", _user_id);

                    MySqlDataReader reader = command.ExecuteReader();

                    string userID = "";
                    string userPass = "";

                    while (reader.Read()) {
                        userID = reader [ "user_id" ].ToString();
                        userPass = reader [ "password_hash" ].ToString();
                    }

                    Console.WriteLine("User ID: " + userID + "  User PswdHash: " + userPass);
                }
                catch (Exception e) {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }
        }

        /// <summary>
        /// SELECT * FROM users
        /// </summary>
        public void ConsoleSelect () {
            using (MySqlConnection connection = CreateConnection()) {
                try {
                    connection.Open();
                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "SELECT * FROM users";
                    try {
                        MySqlDataReader reader = command.ExecuteReader();

                        string userID = "";
                        string userPass = "";

                        while (reader.Read()) {
                            userID = reader [ "user_id" ].ToString();
                            userPass = reader [ "password_hash" ].ToString();
                            Console.WriteLine("User ID: " + userID + "  User PswdHash: " + userPass);
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine("ERROR: " + e.Message);
                    }
                }
                catch (Exception e) {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }
        }

        public void ConsoleInsert (string _user_id, string _password_hash) {
            using (MySqlConnection connection = CreateConnection()) {
                try {
                    connection.Open();

                    MySqlCommand command = connection.CreateCommand();

                    command.CommandText = "INSERT INTO users (user_id, password_hash) VALUES (@user_id, @password_hash)";

                    command.Parameters.AddWithValue("@user_id", _user_id);
                    command.Parameters.AddWithValue("@password_hash", _password_hash);

                    try {
                        int rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine("Rows affected: " + rowsAffected.ToString());
                    }
                    catch (Exception e) {

                        Console.WriteLine("ERROR: " + e.Message);
                    }
                }
                catch (Exception e) {
                    Console.WriteLine("ERROR: " + e.Message);
                }
            }
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
                    if (!string.IsNullOrEmpty(userID))
                        return new UserModel() { UserID = userID, PswdHash = userPass, CreatedAt = createdAt, RequestType = GlobalVariablesLib.RequestTypes.Get_User, Status = GlobalVariablesLib.RequestStatus.Success };

                    else {
                        Console.WriteLine("User with ID " + _user.UserID + " does not exist, or couldn't be found");
                        return new UserModel() { UserID = _user.UserID, RequestType = GlobalVariablesLib.RequestTypes.Get_User, Status = GlobalVariablesLib.RequestStatus.DoesNotExist };
                    }
                }
                catch (Exception e) {
                    Console.WriteLine("ERROR: " + e.Message);
                    return new UserModel() { UserID = _user.UserID, RequestType = GlobalVariablesLib.RequestTypes.Get_User, Status = GlobalVariablesLib.RequestStatus.ConnectionError };
                }
            }
        }

        ///// <summary>
        ///// SELECT * FROM users
        ///// </summary>
        //public List<User> Select () {
        //    using (MySqlConnection connection = CreateConnection()) {
        //        try {
        //            connection.Open();
        //            MySqlCommand command = connection.CreateCommand();

        //            command.CommandText = "SELECT * FROM users";
        //            try {
        //                MySqlDataReader reader = command.ExecuteReader();

        //                List<User> users = new List<User>();
        //                string userID = "";
        //                string userPass = "";

        //                while (reader.Read()) {
        //                    userID = reader [ "user_id" ].ToString();
        //                    userPass = reader [ "password_hash" ].ToString();
        //                    Console.WriteLine("User ID: " + userID + "  User PswdHash: " + userPass);
        //                    users.Add(new User() { UserID = userID, PswdHash = userPass });
        //                }
        //                return users;
        //            }
        //            catch (Exception e) {
        //                Console.WriteLine("ERROR: " + e.Message);
        //                return null;
        //            }
        //        }
        //        catch (Exception e) {
        //            Console.WriteLine("ERROR: " + e.Message);
        //            return null;
        //        }
        //    }
        //}

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
                        Console.WriteLine("User " + _user.UserID + " inserted");
                        return new UserModel() { UserID = _user.UserID, PswdHash = _user.PswdHash, RequestType = GlobalVariablesLib.RequestTypes.Create_User, Status = GlobalVariablesLib.RequestStatus.Success };
                    }
                    catch (Exception e) {
                        Console.WriteLine("ERROR: " + e.Message);
                        return new UserModel() { UserID = _user.UserID, PswdHash = _user.PswdHash, RequestType = GlobalVariablesLib.RequestTypes.Create_User, Status = GlobalVariablesLib.RequestStatus.AlreadyExists };
                    }
                }
                catch (Exception e) {
                    Console.WriteLine("ERROR: " + e.Message);
                    return new UserModel() { UserID = _user.UserID, PswdHash = _user.PswdHash, RequestType = GlobalVariablesLib.RequestTypes.Create_User, Status = GlobalVariablesLib.RequestStatus.ConnectionError };
                }
            }
        }
    }
}