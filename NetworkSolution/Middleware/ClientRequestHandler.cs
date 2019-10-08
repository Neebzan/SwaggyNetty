using GlobalVariablesLib;
using MSMQHelperUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpHelper;
using static Middleware.Middleware_Main;


namespace Middleware {
    class ClientRequestHandler {
        private TcpClient client;
        private NetworkStream stream;
        private string uniqueClientID;

        private UserModel ErrorModel { get; set; }

        /// <summary>
        /// Connection check property. Returns a true if the TcpClient is currently connected, else returns false.
        /// Credit: Robert Conan McMillan
        /// </summary>
        public bool Connected {
            get {
                try {
                    if (client.Client != null && client.Client.Connected) {
                        if (client.Client.Poll(0, SelectMode.SelectRead)) {
                            byte[] buff = new byte[1];
                            if (client.Client.Receive(buff, SocketFlags.Peek) == 0) {
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

        public ClientRequestHandler(TcpClient client) {
            Init(client);
        }

        /// <summary>
        /// Handles Incoming Request from a connected TcpClient
        /// </summary>
        public void HandleRequest() {
            // Sets Current Thread Name for debugging purposes.
            Thread.CurrentThread.Name = $"REQUEST_HANDLER[{uniqueClientID}]";
            #region Timeout timer
            // Local timer, to check age of instance and close instance if too old.
            Stopwatch timeout = new Stopwatch();
            timeout.Start();
            #endregion
            // First we check for an active connection, to avoid crashing if conenction is lost.
            while (Connected) {
                // We recieve data from Networkstream
                string recieved = MessageFormatter.ReadStreamOnce(stream);

                // if not null or empty, presumably useable data has been recieved
                if (!String.IsNullOrEmpty(recieved)) {
                    try {
                        // Sets a local UserModel to deserialised recieved data.
                        UserModel userRequest = JsonConvert.DeserializeObject<UserModel>(recieved);

                        // Hashes password if it is not null or empty.
                        if (!String.IsNullOrEmpty(userRequest.PswdHash)) {
                            userRequest.PswdHash = GetPasswordHash(userRequest.PswdHash);
                        }

                        // Sets Unique ID for the UserModel.
                        userRequest.RemoteEndPoint = uniqueClientID;

                        // Create a Messages With Recieved Data in Body.
                        Message request = new Message() {
                            Body = JsonConvert.SerializeObject(userRequest), Formatter = new JsonMessageFormatter()
                        };

                        // Switch control, to figure out where to send Messages.
                        switch (userRequest.RequestType) {
                            case RequestType.Get_User:
                                // Enqueues Message to Database Message Queue
                                EnqueueMessage(request, userRequest.UserID, databaseRequestQueue);

                                // Within A specific amount of time, tries to recieve specific message with unique id from Database message queue
                                if (GetMessage(userRequest.UserID, uniqueClientID, dataBaseResponses,out Message response)){
                                    // Model Representing response.
                                    UserModel databaseResponseModel = JsonConvert.DeserializeObject<UserModel>(response.Body.ToString());
                                    // Calls statushandler, that based on UserModel.Status Returns True, if Success, or false and an
                                    // error message for the user.
                                    if (StatusHandler(databaseResponseModel,out string message)) {
                                        if (userRequest.PswdHash == databaseResponseModel.PswdHash) {
                                            // Queue Where Exptected Messages should be recieved from.
                                            response.ResponseQueue = tokenResponseQueue;
                                            // Enqueues message to Token System.
                                            EnqueueMessage(response, TokenRequestType.CreateToken.ToString(), tokenRequestQueue);

                                            // Get Message From Token System Containing Token
                                            if(GetMessage(userRequest.UserID,uniqueClientID,tokenResponses,out Message tokenResponse)) {
                                                SendToClient(tokenResponse.Body.ToString());
                                            }
                                            /// TIMEOUT ERROR
                                            else {
                                                // Sets ErrorModel.Message Variable
                                                ErrorModel.Message = $"ERROR: Request Timed Out, Please Try Again.";
                                                // Sends the client an error message 
                                                SendToClient(JsonConvert.SerializeObject(ErrorModel));
                                                WriteLine("Error: GetMessage Timed Out", true);
                                            }
                                        } else {
                                            // Sets ErrorModel.Message Variable
                                            ErrorModel.Message = $"ERROR: Wrong Username or Password";
                                            // Sends the client an error message 
                                            SendToClient(JsonConvert.SerializeObject(ErrorModel));
                                            WriteLine($"ERROR: Password-Hash Matching Failed", true);
                                        }
                                    }
                                    /// STATUS ERROR
                                    else {
                                        // Sets ErrorModel.Message Variable
                                        ErrorModel.Message = $"ERROR: {message}";
                                        // Sends the client an error message 
                                        SendToClient(JsonConvert.SerializeObject(ErrorModel));
                                        WriteLine($"ERROR: {message}", true);
                                    }
                                }
                                /// TIMEOUT ERROR
                                else {
                                    // Sets ErrorModel.Message Variable
                                    ErrorModel.Message = $"ERROR: Request Timed Out, Please Try Again.";
                                    // Sends the client an error message 
                                    SendToClient(JsonConvert.SerializeObject(ErrorModel));
                                    WriteLine("Error: GetMessage Timed Out", true);
                                }
                                break;
                            case RequestType.Create_User:
                                // Enqueues Message to Database Message Queue
                                EnqueueMessage(request, userRequest.UserID, databaseRequestQueue);

                                // Within A specific amount of time, tries to recieve specific message with unique id from Database message queue
                                if (GetMessage(userRequest.UserID, uniqueClientID, dataBaseResponses, out Message createUserResponse)) {
                                    // Model Representing response.
                                    UserModel databaseResponseModel = JsonConvert.DeserializeObject<UserModel>(createUserResponse.Body.ToString());
                                    if (StatusHandler(databaseResponseModel, out string message)) {
                                        SendToClient(createUserResponse.Body.ToString());
                                    }
                                    /// STATUS ERROR
                                    else {
                                        // Sets ErrorModel.Message Variable
                                        ErrorModel.Message = $"ERROR: {message}";
                                        // Sends the client an error message 
                                        SendToClient(JsonConvert.SerializeObject(ErrorModel));
                                        WriteLine($"ERROR: {message}", true);
                                    }
                                }
                                /// TIMEOUT ERROR
                                else {
                                    // Sets ErrorModel.Message Variable
                                    ErrorModel.Message = $"ERROR: Request Timed Out, Please Try Again.";
                                    // Sends the client an error message 
                                    SendToClient(JsonConvert.SerializeObject(ErrorModel));
                                    WriteLine("Error: GetMessage Timed Out", true);
                                }
                                break;
                            case RequestType.Token_Check:
                                request.ResponseQueue = tokenResponseQueue;
                                EnqueueMessage(request, TokenRequestType.VerifyToken.ToString(),tokenRequestQueue);
                                if (GetMessage(userRequest.UserID,uniqueClientID,tokenResponses,out Message verifyTokenMessage)) {
                                    SendToClient(verifyTokenMessage.Body.ToString());
                                }
                                /// TIMEOUT ERROR
                                else {
                                    // Sets ErrorModel.Message Variable
                                    ErrorModel.Message = $"ERROR: Request Timed Out, Please Try Again.";
                                    // Sends the client an error message 
                                    SendToClient(JsonConvert.SerializeObject(ErrorModel));
                                    WriteLine("Error: GetMessage Timed Out", true);
                                }
                                break;
                            default:
                                // Sets ErrorModel.Message Variable
                                ErrorModel.Message = $"UNEXPECTED ERROR!";
                                // Sends the client an error message 
                                SendToClient(JsonConvert.SerializeObject(ErrorModel));
                                WriteLine("Error: UNEXPECTED ERROR", true);
                                break;
                        }
                    } catch (Exception e) {
                        // Sets ErrorModel.Message Variable
                        ErrorModel.Message = $"ERROR: {e.Message}";
                        // Sends the client an error message 
                        SendToClient(JsonConvert.SerializeObject(ErrorModel));
                        WriteLine($"Error: {e.Message}",true);
                    }
                }
                // If too old, close connection.
                else if (timeout.Elapsed.TotalSeconds > MaxMessageLifeTime) {
                    if (Connected) {
                        client.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Initialises the ClientRequestHandler
        /// </summary>
        /// <param name="client"></param>
        private void Init(TcpClient client) {
            // Sets local TcpClient reference to client reference from Constructor parameter
            this.client = client;

            // Connected Networkstream
            stream = client.GetStream();

            // Unique ID created using Remote IP and Port via RemoteEndPoint
            uniqueClientID = client.Client.RemoteEndPoint.ToString();

            // Model to Send Back Error Information to Client
            ErrorModel = new UserModel() {
                RequestType = RequestType.Error
            };
        }

        /// <summary>
        /// Password has function from Esben Juhl Dalsgaard
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private string GetPasswordHash(string password) {
            using (HMACSHA512 t = new HMACSHA512(Encoding.UTF8.GetBytes(password))) {
                byte[] hash;
                hash = t.ComputeHash(Encoding.UTF8.GetBytes(password));
                password = BitConverter.ToString(hash).Replace("-", "");
            }
            return password;
        }

        /// <summary>
        /// Sends a string as a bytestream to the user.
        /// </summary>
        /// <param name="value">data to send</param>
        private void SendToClient(string value) {
            if (Connected) {
                // Get bytes, Using The TcpHelper.MessageFormatter.MessageBytes method to get the correct header.
                byte[] msg = MessageFormatter.MessageBytes(value);
                // Writes byte array to Networkstream
                stream.Write(msg, 0, msg.Length);
            }
        }

        /// <summary>
        /// Returns True if the status was success, otherwise returns false else.
        /// Also throws out a message, to describe the outcome
        /// </summary>
        /// <param name="userModel"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        private bool StatusHandler(UserModel userModel,out string Message) {
            switch (userModel.Status) {
                case RequestStatus.Success:
                    Message = "User Exists";
                    return true;
                case RequestStatus.AlreadyExists:
                    Message = "Cannot Create User, Already Exists";
                    return false;
                case RequestStatus.DoesNotExist:
                    Message = "Cannot Find User";
                    return false;
                case RequestStatus.ConnectionError:
                    Message = "The database encountered an Unexpected Connection Error";
                    return false;
            }
            Message = "Unexpected Error";
            return false;
        }

    }
}