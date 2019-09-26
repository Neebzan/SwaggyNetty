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

namespace Login_Middleware
{
    /// <summary>
    /// Middleware_Client represents a connected client internally
    /// </summary>
    class Middleware_Client
    {
        private TcpClient tcpClient;
        private MessageQueue databaseRequestQueue, databaseResponseQueue, tokenRequestQueue, tokenResponseQueue;
        private Json_Obj user_obj;
        private NetworkStream stream;
        private bool isAlive;

        public Middleware_Client(TcpClient client)
        {
            Console.WriteLine("Middleware_Client Created!");

            isAlive = true;
            if (!MessageQueue.Exists(".\\private$\\TestQueue"))
            {
                MessageQueue.Create(".\\private$\\TestQueue");
            }
            // Sets the correct queue for the client to send and recieve from
            databaseRequestQueue = new MessageQueue(".\\private$\\TestQueue");
            databaseResponseQueue = new MessageQueue(".\\private$\\TestQueue");
            tokenRequestQueue = new MessageQueue(".\\private$\\QueueName");
            tokenResponseQueue = new MessageQueue(".\\private$\\QueueName");



            tcpClient = client;
            stream = tcpClient.GetStream();
        }

        /// <summary>
        /// Using The JsonConvert Class from Newtonsoft.Json; converts a string to a Json_Obj
        /// Made for ease of use, and less code.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private Json_Obj DeserializeRequest(string data)
        {
            return JsonConvert.DeserializeObject<Json_Obj>(data);
        }


        public void ListenForMessages()
        {
            while (isAlive)
            {

                string data = MessageFormatter.ReadMessage(stream);

                Console.WriteLine("{0} Received: {1}\nAttempting To Convert To Json Object", this.ToString(), data);
                try
                {
                    user_obj = DeserializeRequest(data);

                    // Queues request from client to db
                    QueueRequest(user_obj);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"EXCEPTION:-----------------------------------\n {e}\n-----------------------------------");
                    Console.WriteLine("\n\n-----------------------------------\n" + data + "\n\n-----------------------------------");

                    // Json client response setup
                    Json_Obj response = new Json_Obj()
                    {
                        Message = $"ERROR: [ {e.Message} ] \nHost Closed the Connection!",
                        RequestType = Json_Obj.RequestTypes.Error
                    };

                    // Get bytes from data (string)

                    byte[] msg = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(response));

                    //sends message to client

                    stream.Write(msg, 0, msg.Length);

                    isAlive = false;
                    tcpClient.Close();
                    Thread.CurrentThread.Abort();
                }
            }
        }


        private bool CheckHash(Json_Obj received_obj, Json_Obj userData)
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


        private Message RequestToken(Json_Obj databaseMessageObj)
        {
            //tokenRequestQueue.Send("");
            //tokenResponseQueue.Receive();

            return new Message();
        }

        private void QueueRequest(Json_Obj userImputData)
        {
            bool success = false;
            string userImputRequestString = JsonConvert.SerializeObject(userImputData);

            switch (userImputData.RequestType)
            {
                case Json_Obj.RequestTypes.Get_User:

                    // Setup DB request
                    Message getRequest = new Message()
                    {
                        Body = userImputRequestString,
                        Label = userImputData.UserID,
                        Formatter = new JsonMessageFormatter()
                    };
                    // Send DB request
                    Console.WriteLine("Queing Request...");
                    databaseRequestQueue.Send(getRequest);
                    {
                        // Json client response setup
                        Json_Obj response = new Json_Obj()
                        {
                            Message = $"Login Request For: [ {userImputData.UserID} ], Sent to Database",
                            RequestType = Json_Obj.RequestTypes.Response
                        };

                        // Get bytes from data (string)
                        byte[] msg = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(response));
                        //sends message to client
                        stream.Write(msg, 0, msg.Length);
                    }

                    // Wait for response
                    while (!success)
                    {
                        if (databaseResponseQueue.Peek().Label == userImputData.UserID)
                        {

                            Message msg = databaseRequestQueue.Receive();
                            msg.Formatter = new JsonMessageFormatter();

                            Json_Obj dataBaseResponseObj = DeserializeRequest(msg.Body.ToString());
                            Console.WriteLine(dataBaseResponseObj.ToString());

                            if (dataBaseResponseObj.Status == Json_Obj.RequestStatus.Success && CheckHash(dataBaseResponseObj, userImputData))
                            {
                                RequestToken(dataBaseResponseObj);
                            }
                            else
                            {
                                string errorMessage = "";
                                switch (dataBaseResponseObj.Status)
                                {
                                    case Json_Obj.RequestStatus.Success:
                                        errorMessage = "Wrong Username or Password";
                                        break;
                                    case Json_Obj.RequestStatus.DoesNotExist:
                                        errorMessage = "Wrong Username or Password";
                                        break;
                                    case Json_Obj.RequestStatus.ConnectionError:
                                        errorMessage = "Connection Error!";
                                        break;
                                    default:
                                        errorMessage = "Unexpected Failure!";
                                        break;
                                }
                                // Json client response setup
                                Json_Obj response = new Json_Obj()
                                {
                                    Message = $"ERROR: {errorMessage}.\n Please Try again",
                                    RequestType = Json_Obj.RequestTypes.Error,
                                    Status = dataBaseResponseObj.Status
                                };

                                // Get bytes from data (string)
                                byte[] msgError = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(response));

                                //sends message to client
                                stream.Write(msgError, 0, msgError.Length);

                                // Terminate Connection and User Instance!
                                isAlive = false;
                                tcpClient.Close();
                                Thread.CurrentThread.Abort();
                            }
                            success = true;
                            isAlive = false;
                            tcpClient.Close();
                            Thread.CurrentThread.Abort();
                        }
                    }
                    break;
                case Json_Obj.RequestTypes.Create_User:
                    // Setup DB request
                    Message createRequest = new Message()
                    {
                        Body = userImputRequestString,
                        Label = userImputData.UserID,
                        Formatter = new JsonMessageFormatter()
                    };
                    // Send DB request
                    Console.WriteLine("Queing Request...");
                    databaseRequestQueue.Send(createRequest);
                    {
                        // Json client response setup
                        Json_Obj response = new Json_Obj()
                        {
                            Message = $"Signup Request For: [ {userImputData.UserID} ], Sent to Database",
                            RequestType = Json_Obj.RequestTypes.Response
                        };

                        // Get bytes from data (string)
                        byte[] msg = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(response));
                        //sends message to client
                        stream.Write(msg, 0, msg.Length);
                    }



                    break;
                case Json_Obj.RequestTypes.Update_User:
                    break;
                case Json_Obj.RequestTypes.Delete_User:
                    break;
                case Json_Obj.RequestTypes.Response:
                    break;
                default:
                    break;
            }

        }

    }
}
