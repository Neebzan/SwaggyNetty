using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalVariablesLib
{
    public enum TokenResponse { Valid, Invalid, Created }
    public enum TokenRequestType { VerifyToken, CreateToken }
    public enum RequestTypes { Get_User, Create_User, Update_User, Delete_User, Response, Error, Token_Get, Token_Check }
    public enum RequestStatus { Success, AlreadyExists, DoesNotExist, ConnectionError }
    public enum SessionRequest { GetAllSessions, GetUserSession, GetOnlineSessions, SetStatus }


    public static class GlobalVariables
    {
        public const string TOKEN_INPUT_QUEUE_NAME = "Token_Input_Queue";
        public const string TOKEN_RESPONSE_QUEUE_NAME = "Token_Response_Queue";
        public const string BEACON_INPUT_QUEUE_NAME = "Beacon_Input_Queue";
        public const string BEACON_RESPONSE_QUEUE_NAME = "Beacon_Response_Queue";
        public const string TEST_QUEUE_NAME = "Test_Queue";

        public const string MIDDLEWARE_IP = "178.155.161.248";
        public const int MIDDLEWARE_PORT = 13010;

        public const string PATCHMANAGER_IP = "178.155.161.248";
        public const int PATCHMANAGER_PORT = 13011;

        public const string CONSUMER_QUEUE_NAME = "userdb_request_consumer";
        public const string PRODUCER_QUEUE_NAME = "userdb_request_producer";

        public const int TOKENSYSTEM_PORT = 13005;
        public const int BEACON_PORT = 13006;
        public const int SESSION_USER_PORT = 13007;
        public const int SESSION_SERVER_PORT = 13008;


        public const int MYSQL_LOGIN_DB_PORT = 3306;
        public const string MYSQL_LOGIN_DB_IP = "178.155.161.248";
        public const string MYSQL_LOGIN_DB_DATABASENAME = "authentication_data";
        public const string MYSQL_LOGIN_DB_USERNAME = "netty";
        public const string MYSQL_LOGIN_DB_PASSWORD = "swaggynetty";
        public static string MYSQL_PLAYER_CONSUMER_QUEUE_NAME = "playerdb_request_consumer";
        public static string MYSQL_PLAYER_PRODUCER_QUEUE_NAME = "playerdb_request_producer";
    }
}
