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
using JWTlib;
using System.Diagnostics;

namespace Login_Middleware {
    /// <summary>
    /// Middleware_Client Represents a User with a TCP Connection internally
    /// </summary>
    class Middleware_Client {
        private TcpClient connectedClient;
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
                    if (connectedClient.Client != null && connectedClient.Client.Connected) {
                        if (connectedClient.Client.Poll(0, SelectMode.SelectRead)) {
                            byte[] buff = new byte[1];

                            if (connectedClient.Client.Receive(buff, SocketFlags.Peek) == 0) {
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
            Console.WriteLine($"Middleware_Client Created!");
            connectedClient = client;
            if (Connected) {
                stream = connectedClient.GetStream();
            }
        }
        ~Middleware_Client() {

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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (isAlive) {
                if (Connected) {
                    string data = MessageFormatter.ReadStreamOnce(stream);
                    if (!String.IsNullOrEmpty(data)) {
                        stopwatch.Reset();

                        // Console info when recieveing data.
                        WriteLine(
                            $"\nRequest Recieved\n_______________________________\n" +
                            $"Received Data: {data}\nAttempting To Convert from Json String\n" +
                            $"_______________________________\n");


                        // Tries to convert recieved data to an object.
                        try {
                            // Queues request from client to db with object, containing recieved data from client
                            QueueRequest(data);
                        } catch (Exception e) {
                            WriteLine($"EXCEPTION:\n" +
                                $"-----------------------------------\n" +
                                $"{e}\n" +
                                $"-----------------------------------\n" +
                                $"-----------------------------------\n" +
                                $"{data}\n" +
                                $"-----------------------------------\n");

                            // Json client response setup
                            UserModel response = new UserModel() {
                                Message = $"ERROR: [ {e.Message} ] \nHost Closed the Connection!",
                                RequestType = RequestType.Error
                            };

                            WriteToClient(JsonConvert.SerializeObject(response));

                            if (Connected) {
                                WriteLine("Closing TCP Connection");
                                connectedClient.Close();
                            }
                        } finally {
                            if (!Connected) {
                                WriteLine("Connection Closed");
                            }
                            isAlive = false;
                        }
                    } else {
                        if (stopwatch.Elapsed > TimeSpan.FromMilliseconds(10000)) {
                            UserModel timeOut = new UserModel() {
                                RequestType = RequestType.Error, Message = "Request Timed Out"
                            };
                            WriteToClient(JsonConvert.SerializeObject(timeOut));
                            isAlive = false;
                            if (Connected) {
                                connectedClient.Close();
                            }
                        } else {
                            Thread.Sleep(500);
                        }
                    }
                } else {
                    isAlive = false;
                }
            }
            Console.WriteLine($"Middleware[{ThreadID}] Task: ListenForMessages() Finished all Work\n");
            isAlive = false;
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
                    WriteLine($"Hashing Successful!");
                    return true;
                } else {
                    WriteLine($"Hashing Failed\n \n{received_obj.PswdHash} =/=\n{userData.PswdHash}\n ");
                    return false;
                }
            } catch (Exception e) {
                WriteLine($"Error: {e.Message}!");

                return false;
            }
        }

        /// <summary>
        /// Handles Error responses from database queue.
        /// </summary>
        /// <param name="errorObject"></param>
        /// <returns></returns>
        private string HandleError(UserModel errorObject) {
            string errorMessage;
            switch (errorObject.Status) {
                case RequestStatus.Success:
                    errorMessage = "Wrong Username or Password";
                    break;
                case RequestStatus.AlreadyExists:
                    errorMessage = $"An Account With Username: {errorObject.UserID}, Already Exists!";
                    break;
                case RequestStatus.DoesNotExist:
                    errorMessage = "Username or Password does not Match, or user doesn't exist!";
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
            // Switch to determine the type of return expected
            switch (tokenRequestType) {
                case TokenRequestType.VerifyToken:
                    databaseMessageObj.RequestType = RequestType.Token_Check;
                    break;
                case TokenRequestType.CreateToken:
                    databaseMessageObj.RequestType = RequestType.Token_Get;
                    break;
            }

            Message tokenRequestMessage = new Message() {
                Formatter = new JsonMessageFormatter(),
                Label = tokenRequestType.ToString(),
                ResponseQueue = Middleware_Main.tokenResponseQueue,
                Body = JsonConvert.SerializeObject(databaseMessageObj)
            };
            // Sends request to token server
            Middleware_Main.tokenRequestQueue.Send(tokenRequestMessage);

            // Awaits response from token server, peeks the top queue stack.
            while (true) {
                Message peekedMessage = Middleware_Main.tokenResponseQueue.Peek();
                peekedMessage.Formatter = new JsonMessageFormatter();
                UserModel peekedModel = DeserializeRequest(peekedMessage.Body.ToString());

                if (peekedMessage.Label == databaseMessageObj.UserID && peekedModel.RemoteEndPoint == databaseMessageObj.RemoteEndPoint) {
                    Message recievedMessage = Middleware_Main.tokenResponseQueue.Receive();
                    recievedMessage.Formatter = new JsonMessageFormatter();

                    UserModel tokenUserModel = DeserializeRequest(recievedMessage.Body.ToString());
                    //tokenUserModel.UserID = databaseMessageObj.UserID;
                    return tokenUserModel;
                }
            }
        }
        private void WriteToClient(string message) {
            if (Connected) {
                // Get bytes, Using The TcpHelper.MessageFormatter.MessageBytes Function
                byte[] msg = MessageFormatter.MessageBytes(message);
                // Sends message to client
                stream.Write(msg, 0, msg.Length);
            }
        }
        private void WriteLine(string message) {
#if DEBUG
            Console.WriteLine($"Middleware_Client at ThreadID: {ThreadID}. " + message);
#endif
        }
        private void QueueRequest(string userImputData) {
            // Deserialises to local obj
            UserModel userImputModel = DeserializeRequest(userImputData);
            userImputModel.RemoteEndPoint = connectedClient.Client.RemoteEndPoint.ToString();

            WriteLine("Object Deserialisation Successful!");
            if (!String.IsNullOrEmpty(userImputModel.PswdHash)) {
                userImputModel.PswdHash = GetPasswordHash(userImputModel.PswdHash);
            }
            userImputData = JsonConvert.SerializeObject(userImputModel);

            // Bool used to determine when the correct message has been recieved, to end message peek loop
            bool success = false;
            //string userImputRequestString = JsonConvert.SerializeObject(userImputData);

            switch (userImputModel.RequestType) {
                /// Case: Requests User information from the Database, based on recieved user data
                case RequestType.Get_User:

                    // Setup DB request message
                    Message getRequest = new Message() {
                        Body = userImputData,
                        Label = userImputModel.UserID,
                        Formatter = new JsonMessageFormatter()
                    };

                    // Send DB request
                    WriteLine("Queing Login Request...");
                    Middleware_Main.databaseRequestQueue.Send(getRequest);

                    WriteLine("Login Request Sent To Database");

                    // Json client response setup
                    UserModel login_partial_response = new UserModel() {
                        Message = $"Login Request For: [ {userImputModel.UserID} ], Sent to Database",
                        RequestType = RequestType.Response
                    };
                    WriteToClient(JsonConvert.SerializeObject(login_partial_response));

                    // Wait for response
                    // While it has no success try, to recieve message from database producer queue with peeking
                    // to make sure it only takes the correct message...
                    WriteLine("Waiting on Response from Database");
                    while (!success) {
                        // Peeks top of queue, and sets the right formatter
                        Message peekedMessage = Middleware_Main.databaseResponseQueue.Peek(TimeSpan.FromMinutes(2));
                        peekedMessage.Formatter = new JsonMessageFormatter();
                        UserModel peekedModel = DeserializeRequest(peekedMessage.Body.ToString());
                        // WriteLine($"\nExpected ID: {userImputData.UserID}\nActual ID: {peekedModel.UserID}\nLabel ID: {peekedMessage.Label}\n");
                        // if the label is as expected, and the request type is the same, consume message
                        // specifically made to be sure a user making two requests at once, can't get the wrong message back.
                        if (peekedMessage.Label == userImputModel.UserID && peekedModel.RemoteEndPoint == connectedClient.Client.RemoteEndPoint.ToString() && peekedModel.RequestType == RequestType.Get_User) {
                            Message actualMessage = Middleware_Main.databaseResponseQueue.Receive();
                            actualMessage.Formatter = new JsonMessageFormatter();

                            UserModel dataBaseResponseObj = DeserializeRequest(actualMessage.Body.ToString());
                            WriteLine("Response Message Recieved from Database, Requesting Token From Token Server");

                            if (dataBaseResponseObj.Status == RequestStatus.Success && CheckHash(dataBaseResponseObj, userImputModel)) {
                                UserModel tokenReponse = RequestToken(userImputModel, TokenRequestType.CreateToken);
                                tokenReponse.Status = RequestStatus.Success;
                                WriteLine("Token Successfully Created And Retrieved!");
                                WriteToClient(JsonConvert.SerializeObject(tokenReponse));
                            } else {
                                #region Access Denied Handling
                                WriteLine($"ERROR: {HandleError(dataBaseResponseObj)}.\n---ACCESS DENIED---");
                                // Json client response setup
                                UserModel response = new UserModel() {
                                    Message = $"Access Denied: {HandleError(dataBaseResponseObj)}.\n Please Try again",
                                    RequestType = RequestType.Error,
                                    Status = dataBaseResponseObj.Status
                                };
                                WriteToClient(JsonConvert.SerializeObject(response));
                                WriteLine("Login Details and Toke Sent to User.");
                                #endregion
                            }
                            success = true;
                        }
                    }
                    break;

                /// Case: Requests Create call on data base, based on recieved user data
                case RequestType.Create_User:
                    WriteLine("User Issued a Signup Request");
                    // Setup of DB request message
                    Message createRequest = new Message() {
                        Body = userImputData,
                        Label = userImputModel.UserID,
                        Formatter = new JsonMessageFormatter()
                    };

                    // Sending DB request message to message queue
                    // Debug Message
                    WriteLine("Queing Signup Request");
                    Middleware_Main.databaseRequestQueue.Send(createRequest);
                    WriteLine("Request Successfully Sent to Database");
                    // Client Response Message
                    // Json client response setup
                    UserModel createResponse = new UserModel() {
                        Message = $"Signup Request For: [ {userImputModel.UserID} ], Sent to Database",
                        RequestType = RequestType.Response,
                        Status = RequestStatus.Success
                    };
                    WriteToClient(JsonConvert.SerializeObject(createResponse));

                    WriteLine("Waiting on response from Database");
                    while (!success) {
                        Message peekedMessage = Middleware_Main.databaseResponseQueue.Peek();
                        peekedMessage.Formatter = new JsonMessageFormatter();
                        UserModel peekedModel = DeserializeRequest(peekedMessage.Body.ToString());
                        // Peeks top of queue, and only when it's the right pulls it from the queue;
                        if (peekedMessage.Label == userImputModel.UserID && peekedModel.RemoteEndPoint == userImputModel.RemoteEndPoint && peekedModel.RequestType == RequestType.Create_User) {
                            Message m = Middleware_Main.databaseResponseQueue.Receive();
                            m.Formatter = new JsonMessageFormatter();
                            UserModel userModel = DeserializeRequest(m.Body.ToString());
                            WriteLine("Response from Database Recieved");
                            switch (userModel.Status) {
                                case RequestStatus.Success:
                                    userModel = RequestToken(DeserializeRequest(m.Body.ToString()), TokenRequestType.CreateToken);
                                    userModel.RequestType = RequestType.Create_User;
                                    userModel.Status = RequestStatus.Success;
                                    WriteLine("User Created Sucessfully!");
                                    break;
                                default:
                                    WriteLine($"Error: {HandleError(userModel)}");
                                    userModel.RequestType = RequestType.Error;
                                    userModel.Message = $"Error: {HandleError(userModel)}";
                                    break;
                            }
                            WriteToClient(JsonConvert.SerializeObject(userModel));
                            success = true;
                        }
                    }
                    break;

                //case RequestTypes.Update_User:
                //    break;
                //case RequestTypes.Delete_User:
                //    break;
                case RequestType.Token_Check:
                    WriteLine("Token Login Request Recieved");
                    WriteLine("Sending Verification Request to Token server");
                    WriteToClient(JsonConvert.SerializeObject(RequestToken(userImputModel, TokenRequestType.VerifyToken)));
                    WriteLine("Response From Token Server Sent to User");
                    break;
                default:
                    success = true;
                    break;
            }
            if (Connected) {
                isAlive = false;
                connectedClient.Close();
            }
        }
    }
}