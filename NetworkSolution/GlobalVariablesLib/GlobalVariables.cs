﻿using System;
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

        public const string LOADBALANCER_IP = "178.155.161.248";

        public const string CONSUMER_QUEUE_NAME = "userdb_request_consumer";
        public const string PRODUCER_QUEUE_NAME = "userdb_request_producer";

        public const int TOKENSYSTEM_PORT = 13005;
        public const int BEACON_PORT = 13006;
        public const int SESSION_USER_PORT = 13007;
        public const int SESSION_SERVER_PORT = 13008;
        public const int GAME_DATABASE_LOADBALANCER_PORT = 13009;



        public const int MYSQL_LOGIN_DB_PORT = 3306;
        public const string MYSQL_LOGIN_DB_IP = "178.155.161.248";
        public const string MYSQL_LOGIN_DB_DATABASENAME = "authentication_data";
        public const string MYSQL_LOGIN_DB_USERNAME = "netty";
        public const string MYSQL_LOGIN_DB_PASSWORD = "swaggynetty";
        public const string MYSQL_PLAYER_DB_CONSUMER_QUEUE_NAME = "playerdb_request_consumer";
        public const string MYSQL_PLAYER_DB_PRODUCER_QUEUE_NAME = "playerdb_request_producer";

        #region MySQL Player database
        public const string MYSQL_PLAYER_DB_MASTER_USERNAME = "replication_user";
        public const string MYSQL_PLAYER_DB_SLAVE_USERNAME = "replication_reader";
        public const string MYSQL_PLAYER_DB_MASTER_PASSWORD = "swaggynetty";
        public const string MYSQL_PLAYER_DB_SLAVE_PASSWORD = "swaggynetty";
        public const string MYSQL_PLAYER_DB_DATABASENAME = "player_data";
        public const string MYSQL_PLAYER_DB_IP = "178.155.161.248";
        public const int MYSQL_PLAYER_DB_MASTER_PORT = 3307;
        public const int MYSQL_PLAYER_DB_SLAVE1_PORT = 3308;
        public const int MYSQL_PLAYER_DB_SLAVE2_PORT = 3309;
        #endregion

    }
}
