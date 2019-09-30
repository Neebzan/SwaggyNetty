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

namespace Login_Middleware
{
    /// <summary>
    /// Middleware_Client represents a connected client internally
    /// </summary>
    class Middleware_Client
    {
        private TcpClient tcpClient;

        private UserModel user_obj;
        private NetworkStream stream;
        private bool isAlive;

        public bool Connected
        {
            get
            {
                try
                {
                    if (tcpClient.Client != null && tcpClient.Client.Connected)
                    {
                        if (tcpClient.Client.Poll(0, SelectMode.SelectRead))
                        {
                            byte[] buff = new byte[1];

                            if (tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                            {
                                return false;
                            }

                            return true;
                        }

                        return true;
                    }

                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        public Middleware_Client(TcpClient client)
        {
            Console.WriteLine("Middleware_Client Created!");

            isAlive = true;
            // Sets the correct queue for the client to send and recieve from


            tcpClient = client;
            if (Connected)
            {
                stream = tcpClient.GetStream();
            }
        }

        ~Middleware_Client()
        {
            Console.WriteLine("Middleware_Client Aborted");
        }

        /// <summary>
        /// Using The JsonConvert Class from Newtonsoft.Json; converts a string to a Json_Obj
        /// Made for ease of use, and less code.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private UserModel DeserializeRequest(string data)
        {
            return JsonConvert.DeserializeObject<UserModel>(data);
        }


        public void ListenForMessages()
        {
            while (isAlive)
            {
                if (Connected)
                {
                    string data = MessageFormatter.ReadStreamOnce(stream);
                    if (!String.IsNullOrEmpty(data))
                    {

                        // Console info when recieveing data.
                        {
                            Console.WriteLine
                                ("_______________________________\n" +
                                "Received: {0}\nAttempting To Convert To Json Object" +
                                "\n_______________________________", data);
                        }

                        // Tries to convert recieved data to an object.
                        try
                        {
                            // Deserialises to local obj
                            user_obj = DeserializeRequest(data);

                            // Queues request from client to db with object, containing recieved data from client
                            QueueRequest(user_obj);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"EXCEPTION:-----------------------------------\n {e}\n-----------------------------------");
                            Console.WriteLine("\n\n-----------------------------------\n" + data + "\n\n-----------------------------------");

                            // Json client response setup
                            UserModel response = new UserModel()
                            {
                                Message = $"ERROR: [ {e.Message} ] \nHost Closed the Connection!",
                                RequestType = RequestTypes.Error
                            };

                            if (Connected)
                            {
                                // Get bytes from data (string)

                                byte[] msg = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(response));

                                //sends message to client

                                stream.Write(msg, 0, msg.Length);
                            }
                            if (Connected)
                            {
                                tcpClient.Close();
                            }
                            isAlive = false;
                        }
                    }
                }
            }
        }


        private bool CheckHash(UserModel received_obj, UserModel userData)
        {
            try
            {
                if (received_obj.PswdHash == userData.PswdHash)
                {
                    Console.WriteLine($"User: {userData.UserID} Found. Requesting Token from Token Server...");
                    return true;
                }
                else
                {
                    Console.WriteLine("Hashing Failed: Wrong Username or Password");
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}!");

                return false;
            }
        }

        private string HandleError(UserModel ErrorObject)
        {
            string errorMessage = "";
            switch (ErrorObject.Status)
            {
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


        private Message RequestToken(UserModel databaseMessageObj)
        {
            //tokenRequestQueue.Send("");
            //tokenResponseQueue.Receive();

            return new Message();
        }

        private void WriteToClient(string message)
        {
            if (Connected)
            {
                // Get bytes from data (string)
                byte[] msg = Encoding.ASCII.GetBytes(message);
                //sends message to client
                stream.Write(msg, 0, msg.Length);
            }
        }

        private void QueueRequest(UserModel userImputData)
        {
            bool success = false;
            string userImputRequestString = JsonConvert.SerializeObject(userImputData);

            switch (userImputData.RequestType)
            {
                /// Case: Requests User information from the Database, based on recieved user data
                case RequestTypes.Get_User:

                    // Setup DB request message
                    Message getRequest = new Message()
                    {
                        Body = userImputRequestString,
                        Label = userImputData.UserID,
                        Formatter = new JsonMessageFormatter()
                    };
                    // Send DB request
                    Console.WriteLine("Queing Request...");
                    Middleware_Main.databaseRequestQueue.Send(getRequest);

                    {
                        // Json client response setup
                        UserModel response = new UserModel()
                        {
                            Message = $"Login Request For: [ {userImputData.UserID} ], Sent to Database",
                            RequestType = RequestTypes.Response
                        };
                        WriteToClient(JsonConvert.SerializeObject(response));
                    }

                    // Wait for response
                    while (!success)
                    {
                        Message peekedMessage = Middleware_Main.databaseResponseQueue.Peek();
                        peekedMessage.Formatter = new JsonMessageFormatter();
                        // Peeks top of queue, and only when it's the right pulls it from the queue;
                        if (peekedMessage.Label == userImputData.UserID && DeserializeRequest(peekedMessage.Body.ToString()).RequestType == RequestTypes.Get_User)
                        {

                            Message msg = Middleware_Main.databaseResponseQueue.Receive();
                            msg.Formatter = new JsonMessageFormatter();

                            UserModel dataBaseResponseObj = DeserializeRequest(msg.Body.ToString());
                            Console.WriteLine(dataBaseResponseObj.ToString());

                            if (dataBaseResponseObj.Status == RequestStatus.Success && CheckHash(dataBaseResponseObj, userImputData))
                            {
                                RequestToken(dataBaseResponseObj);
                            }
                            else
                            {
                                Console.WriteLine($"ERROR: {HandleError(dataBaseResponseObj)}.\n||||| USER REQUEST DENIED FROM HOST |||||");
                                // Json client response setup
                                UserModel response = new UserModel()
                                {
                                    Message = $"ERROR: {HandleError(dataBaseResponseObj)}.\n Please Try again",
                                    RequestType = RequestTypes.Error,
                                    Status = dataBaseResponseObj.Status
                                };

                                if (Connected)
                                {
                                    // Get bytes from data (string)
                                    byte[] msgError = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(response));

                                    //sends message to client
                                    stream.Write(msgError, 0, msgError.Length);

                                    // Terminate Connection and User Instance!
                                    isAlive = false;
                                }
                            }
                            success = true;
                            if (Connected)
                            {
                                isAlive = false;
                                tcpClient.Close();
                            }
                        }
                    }
                    break;

                /// Case: Requests Create call on data base, based on recieved user data
                case RequestTypes.Create_User:

                    // Setup of DB request message
                    Message createRequest = new Message()
                    {
                        Body = userImputRequestString,
                        Label = userImputData.UserID,
                        Formatter = new JsonMessageFormatter()
                    };

                    // Sending DB request message to message queue
                    // Debug Message
                    Console.WriteLine("Queing Create Request...");
                    Middleware_Main.databaseRequestQueue.Send(createRequest);

                    /// Client Response Message
                    {
                        // Json client response setup
                        UserModel response = new UserModel()
                        {
                            Message = $"Signup Request For: [ {userImputData.UserID} ], Sent to Database",
                            RequestType = RequestTypes.Response,
                            Status = RequestStatus.Success
                        };
                    }

                    Message tempMessage = Middleware_Main.databaseResponseQueue.Peek();
                    tempMessage.Formatter = new JsonMessageFormatter();
                    // Peeks top of queue, and only when it's the right pulls it from the queue;
                    if (tempMessage.Label == userImputData.UserID && DeserializeRequest(tempMessage.Body.ToString()).RequestType == RequestTypes.Create_User)
                    {
                        Message m = Middleware_Main.databaseResponseQueue.Receive();
                        m.Formatter = new JsonMessageFormatter();
                        Console.WriteLine("\nCREATE RESPONSE: " + m.Body.ToString() + "\n");
                    }
                    isAlive = false;

                    break;
                case RequestTypes.Update_User:
                    break;
                case RequestTypes.Delete_User:
                    break;
                case RequestTypes.Response:
                    break;
                default:
                    break;
            }

        }

    }
}
