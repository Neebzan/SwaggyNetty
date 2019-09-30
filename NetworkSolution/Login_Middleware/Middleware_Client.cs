using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Messaging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MSMQHelperUtilities;
using TcpHelper;
using System.IO;
using System.Threading;
using GlobalVariablesLib;
using System.Security.Cryptography;

namespace Login_Middleware {
    /// <summary>
    /// Middleware_Client represents a connected client internally
    /// </summary>
    class Middleware_Client {
        private TcpClient tcpClient;

        private UserModel user_obj;
        private NetworkStream stream;
        private bool isAlive = true;
        private string ThreadID {
            get {
                if (Thread.CurrentThread.IsAlive) {
                    return Thread.CurrentThread.ManagedThreadId.ToString();
                } else {
                    return "";
                }
            }
        }

        /// <summary>
        /// Connection check. Checks if the client is currently connected.
        /// </summary>
        public bool Connected {
            get {
                try {
                    if (tcpClient.Client != null && tcpClient.Client.Connected) {
                        if (tcpClient.Client.Poll(0, SelectMode.SelectRead)) {
                            byte[] buff = new byte[1];

                            if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0) {
                                return false;
                            }

                            return true;
                        }

                        return true;
                    }

                    return false;
                } catch {
                    return false;
                }
            }
        }

        /// <summary>
        /// <para>Middleware_Client Constructor</para>
        /// Needs an already connected TCP Client as parameter
        /// <para>as this class does not include any connecting functionality.</para>
        /// </summary>
        /// <param name="client"></param>
        public Middleware_Client(TcpClient client) {
            Console.WriteLine($"Middleware_Client Created On Thread:\n////\n{Thread.CurrentThread.Name}\n////\n Middleware_Client ID: [ {ThreadID} ]");
            tcpClient = client;
            if (Connected) {
                stream = tcpClient.GetStream();
            }
        }
        ~Middleware_Client() {
            Console.WriteLine($"Middleware_Client[{ThreadID}] Aborted");
        }

        /// <summary>
        /// Using The JsonConvert Class from Newtonsoft.Json; converts a string to a Json_Obj
        /// Made for ease of use, and less code.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private UserModel DeserializeRequest(string data) {
            return JsonConvert.DeserializeObject<UserModel>(data);
        }

        /// <summary>
        /// The ListenForMessages function reads the TcpClient messagestream while isAlive = true
        /// Then parses the message to QueueRequests when a full message has been recieved and successfully
        /// converted from Json format to the User Model. Aborts on lost connection.
        /// </summary>
        public void ListenForMessages() {
            while (isAlive) {
                if (Connected) {
                    string data = MessageFormatter.ReadStreamOnce(stream);
                    if (!String.IsNullOrEmpty(data)) {

                        // Console info when recieveing data.
                        {
                            Console.WriteLine
                                ("_______________________________\n" +
                                "###Middleware_Client###\nReceived: {0}\nAttempting To Convert To Json Object" +
                                "\n_______________________________", data);
                        }

                        // Tries to convert recieved data to an object.
                        try {
                            // Deserialises to local obj
                            user_obj = DeserializeRequest(data);

                            // Queues request from client to db with object, containing recieved data from client
                            QueueRequest(user_obj);
                        } catch (Exception e) {
                            Console.WriteLine($"EXCEPTION:-----------------------------------\n {e}\n-----------------------------------");
                            Console.WriteLine("\n\n-----------------------------------\n" + data + "\n\n-----------------------------------");

                            // Json client response setup
                            UserModel response = new UserModel() {
                                Message = $"ERROR: [ {e.Message} ] \nHost Closed the Connection!",
                                RequestType = RequestTypes.Error
                            };

                            if (Connected) {
                                // Get bytes from data (string)

                                byte[] msg = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(response));

                                //sends message to client

                                stream.Write(msg, 0, msg.Length);
                            }
                            if (Connected) {
                                tcpClient.Close();
                            }
                            isAlive = false;
                        }
                    }
                } else {
                    Console.WriteLine();
                    isAlive = false;
                }
            }
        }

        /// <summary>
        /// Password has function from Esben JD
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private static string GetPasswordHash(string password) {
            using (HMACSHA512 t = new HMACSHA512(Encoding.UTF8.GetBytes(password))) {
                byte[] hash;
                hash = t.ComputeHash(Encoding.UTF8.GetBytes(password));
                password = BitConverter.ToString(hash).Replace("-", "");
            }
            return password;
        }

        /// <summary>
        /// <para>CheckHash takes two UserModels and</para>
        /// using a seperate function, hashes the users password, then compares with database result.
        /// </summary>
        /// <param name="received_obj"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        private bool CheckHash(UserModel received_obj, UserModel userData) {
            // Tries to compare hashed passwords
            try {
                if (received_obj.PswdHash == userData.PswdHash) {
                    Console.WriteLine($"Middleware_Client[{ThreadID}]: User: {userData.UserID} Found. Requesting Token from Token Server...");
                    return true;
                } else {
                    Console.WriteLine($"Middleware_Client[{ThreadID}]: Hashing Failed...\n###################\n{received_obj.PswdHash} ==\n{userData.PswdHash}\n###################");
                    return false;
                }
            } catch (Exception e) {
                Console.WriteLine($"Error in Middleware_Client[{ThreadID}]: {e.Message}!");

                return false;
            }
        }

        private string HandleError(UserModel ErrorObject) {
            string errorMessage = "";
            switch (ErrorObject.Status) {
                case RequestStatus.Success:
                    errorMessage = "Wrong Username or Password";
                    break;
                case RequestStatus.AlreadyExists:
                    break;
                case RequestStatus.DoesNotExist:
                    errorMessage = "Wrong Username or Password";
                    break;
                case RequestStatus.ConnectionError:
                    errorMessage = "Connection Error!";
                    break;
                default:
                    errorMessage = "Unexpected Failure!";
                    break;
            }
            return errorMessage;
        }

        /// <summary>
        /// Queues a Message from the Token server with a request
        /// <para>Can: Get_Token and Check_Token</para>
        /// </summary>
        /// <param name="databaseMessageObj"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        private UserModel RequestToken(UserModel databaseMessageObj, GlobalVariablesLib.TokenRequestType tokenRequestType) {

            UserModel tempModel = databaseMessageObj;

            // Switch to determine the type of return expected
            switch (tokenRequestType) {
                case TokenRequestType.VerifyToken:
                    tempModel.RequestType = RequestTypes.Token_Check;
                    break;
                case TokenRequestType.CreateToken:
                    tempModel.RequestType = RequestTypes.Token_Get;
                    break;
            }

            Message tokenRequestMessage = new Message() {
                Formatter = new JsonMessageFormatter(),
                Label = tokenRequestType.ToString(),
                ResponseQueue = Middleware_Main.tokenResponseQueue,
                Body = JsonConvert.SerializeObject(tempModel)
            };
            Middleware_Main.tokenRequestQueue.Send(tokenRequestMessage);

            bool success = false;
            while (!success) {
                Message peekedMessage = Middleware_Main.tokenResponseQueue.Peek();
                peekedMessage.Formatter = new JsonMessageFormatter();

                if (peekedMessage.Label == databaseMessageObj.UserID && DeserializeRequest(peekedMessage.Body.ToString()).RequestType == tempModel.RequestType) {
                    Message recievedMessage = Middleware_Main.tokenResponseQueue.Receive();
                    recievedMessage.Formatter = new JsonMessageFormatter();

                    UserModel tokenUserModel = DeserializeRequest(recievedMessage.Body.ToString());

                    return tokenUserModel;
                }
            }
            return null;
        }

        private void WriteToClient(string message) {
            if (Connected) {
                // Get bytes from data (string)
                byte[] msg = MessageFormatter.MessageBytes(message);
                //sends message to client
                stream.Write(msg, 0, msg.Length);
            }
        }

        private void QueueRequest(UserModel userImputData) {
            if (!String.IsNullOrEmpty(userImputData.PswdHash)) {
                userImputData.PswdHash = GetPasswordHash(userImputData.PswdHash);
            }

            bool success = false;
            string userImputRequestString = JsonConvert.SerializeObject(userImputData);

            switch (userImputData.RequestType) {
                /// Case: Requests User information from the Database, based on recieved user data
                case RequestTypes.Get_User:

                    // Setup DB request message
                    Message getRequest = new Message() {
                        Body = userImputRequestString,
                        Label = userImputData.UserID,
                        Formatter = new JsonMessageFormatter()
                    };
                    // Send DB request
                    Console.WriteLine("Queing Request...");
                    Middleware_Main.databaseRequestQueue.Send(getRequest);

                    // Json client response setup
                    UserModel login_partial_response = new UserModel() {
                        Message = $"Login Request For: [ {userImputData.UserID} ], Sent to Database",
                        RequestType = RequestTypes.Response
                    };
                    WriteToClient(JsonConvert.SerializeObject(login_partial_response));

                    // Wait for response
                    // While it has no success try, to recieve message from database producer queue with peeking
                    // to make sure it only takes the correct message...
                    while (!success) {
                        // Peeks top of queue, and sets the right formatter
                        Message peekedMessage = Middleware_Main.databaseResponseQueue.Peek();
                        peekedMessage.Formatter = new JsonMessageFormatter();

                        // if the label is as expected, and the request type is the same, consume message
                        // specifically made to be sure a user making two requests at once, can't get the wrong message back.
                        if (peekedMessage.Label == userImputData.UserID && DeserializeRequest(peekedMessage.Body.ToString()).RequestType == RequestTypes.Get_User) {
                            Message msg = Middleware_Main.databaseResponseQueue.Receive();
                            msg.Formatter = new JsonMessageFormatter();

                            UserModel dataBaseResponseObj = DeserializeRequest(msg.Body.ToString());
                            Console.WriteLine(dataBaseResponseObj.ToString());

                            if (dataBaseResponseObj.Status == RequestStatus.Success && CheckHash(dataBaseResponseObj, userImputData)) {
                                UserModel tokenReponse = RequestToken(userImputData, TokenRequestType.CreateToken);
                                tokenReponse.Status = RequestStatus.Success;
                                WriteToClient(JsonConvert.SerializeObject(tokenReponse));
                            } else {
                                Console.WriteLine($"Middleware_Client[{ThreadID}]:\nERROR: {HandleError(dataBaseResponseObj)}.\n||||| USER REQUEST DENIED FROM HOST |||||");
                                // Json client response setup
                                UserModel response = new UserModel() {
                                    Message = $"ERROR: {HandleError(dataBaseResponseObj)}.\n Please Try again",
                                    RequestType = RequestTypes.Error,
                                    Status = dataBaseResponseObj.Status
                                };

                                if (Connected) {
                                    WriteToClient(JsonConvert.SerializeObject(response));
                                    isAlive = false;
                                }
                            }
                            success = true;
                            if (Connected) {
                                isAlive = false;
                                tcpClient.Close();
                            }
                        }
                    }
                    break;

                /// Case: Requests Create call on data base, based on recieved user data
                case RequestTypes.Create_User:
                    // Setup of DB request message
                    Message createRequest = new Message() {
                        Body = userImputRequestString,
                        Label = userImputData.UserID,
                        Formatter = new JsonMessageFormatter()
                    };

                    // Sending DB request message to message queue
                    // Debug Message
                    Console.WriteLine("Queing Create Request...");
                    Middleware_Main.databaseRequestQueue.Send(createRequest);

                    // Client Response Message
                    // Json client response setup
                    UserModel createResponse = new UserModel() {
                        Message = $"Signup Request For: [ {userImputData.UserID} ], Sent to Database",
                        RequestType = RequestTypes.Response,
                        Status = RequestStatus.Success
                    };

                    while (!success) {
                        Message tempMessage = Middleware_Main.databaseResponseQueue.Peek();
                        tempMessage.Formatter = new JsonMessageFormatter();

                        // Peeks top of queue, and only when it's the right pulls it from the queue;
                        if (tempMessage.Label == userImputData.UserID && DeserializeRequest(tempMessage.Body.ToString()).RequestType == RequestTypes.Create_User) {
                            Message m = Middleware_Main.databaseResponseQueue.Receive();
                            m.Formatter = new JsonMessageFormatter();
                            UserModel userModel = DeserializeRequest(m.Body.ToString());

                            switch (userModel.Status) {
                                case RequestStatus.Success:
                                    userModel = RequestToken(DeserializeRequest(m.Body.ToString()), TokenRequestType.CreateToken);
                                    break;
                                default:
                                    break;
                            }
                            WriteToClient(JsonConvert.SerializeObject(userModel));
                            Console.WriteLine($"Middleware_Client[{ThreadID}]\nCREATE RESPONSE: " + m.Body.ToString() + "\n");

                        }
                        if (Connected) {
                            isAlive = false;
                            tcpClient.Close();
                        }
                    }
                    break;

                //case RequestTypes.Update_User:
                //    break;
                //case RequestTypes.Delete_User:
                //    break;
                case RequestTypes.Token_Check:
                    WriteToClient(JsonConvert.SerializeObject(RequestToken(userImputData, TokenRequestType.VerifyToken)));
                    break;
                default:
                    break;
            }

        }

    }
}
