﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalVariablesLib
{
    public enum TokenResponse { Valid, Invalid, Created }
    public enum TokenRequestType { VerifyToken, CreateToken }
    public enum RequestTypes { Get_User, Create_User, Update_User, Delete_User, Response }
    public enum RequestStatus { Success, AlreadyExists, DoesNotExist, ConnectionError }

    public static class GlobalVariables
    {
        public const string TOKEN_INPUT_QUEUE_NAME = "Token_Input_Queue";
        public const string BEACON_INPUT_QUEUE_NAME = "Beacon_Input_Queue";
        public const string BEACON_RESPONSE_QUEUE_NAME = "Beacon_Response_Queue";
        public const string TEST_QUEUE_NAME = "Test_Queue";




        public const string CONSUMER_QUEUE_NAME = "userdb_request_consumer";
        public const string PRODUCER_QUEUE_NAME = "userdb_request_producer";

        public const int TOKENSYSTEM_PORT = 13005;
        public const int BEACON_PORT = 13006;
    }
}
