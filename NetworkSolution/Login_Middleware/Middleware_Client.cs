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
using System.IO;

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

        public Middleware_Client(TcpClient client, MessageQueue postingQueue, MessageQueue receivingQueue)
        {
            // Sets the correct queue for the client to send and recieve from
            databaseRequestQueue = postingQueue;
            databaseResponseQueue = receivingQueue;


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
            return (Json_Obj)JsonConvert.DeserializeObject(data);
        }


        public void ListenForMessages()
        {
            while (true)
            {
                int i = 0;

                Byte[] bytes = new Byte[256];
                String data = null;

                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    data = Encoding.ASCII.GetString(bytes, 0, i);
                    Console.WriteLine("Received: {0}", data);

                    user_obj = DeserializeRequest(data);

                    // Json client response setup
                    Json_Obj response = new Json_Obj()
                    {
                        Message = $"Login Request For: {user_obj.UserID}, Sent to Database",
                        RequestType = Json_Obj.RequestTypes.Response
                    };

                    // Get bytes from data (string)
                    byte[] msg = Encoding.ASCII.GetBytes(data);
                    //sends message to client
                    stream.Write(msg, 0, msg.Length);

                    // Queues request from client to db
                    QueueRequest(user_obj);

                    msg = Encoding.ASCII.GetBytes("");

                    stream.Write(msg, 0, msg.Length);
                }

            }
        }

        private bool CheckHash(Message recieved_data, Json_Obj userData, out string result)
        {
            try
            {
                Json_Obj received_obj = DeserializeRequest(recieved_data.Body.ToString());

                if (received_obj.PswdHash == userData.PswdHash)
                {
                    result = "Success";
                    Console.WriteLine($"User: {userData.UserID} Found. Requesting Token from Token Server...");
                    return true;
                }
                else
                {
                    result = "Wrong Username or Password";
                    Console.WriteLine("Hashing Failed: Wrong Username or Password");
                    return false;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}!");
                result = $"Error: {e}!";
                return false;
            }
        }

        private Message RequestToken(Message databaseMessage)
        {
            return new Message();
        }

        private void QueueRequest(Json_Obj data)
        {
            bool success = false;
            string json_string = JsonConvert.SerializeObject(data);

            switch (data.RequestType)
            {
                case Json_Obj.RequestTypes.Get_User:

                    // Setup DB request
                    Message getReq = new Message()
                    {
                        Label = data.UserID,
                        Body = json_string,
                        Formatter = new JsonMessageFormatter(),
                        
                    };
                    // Send DB request
                    databaseRequestQueue.Send(getReq);

                    // Wait for response
                    while (!success)
                    {
                        if (databaseResponseQueue.Peek().Label == data.UserID)
                        {
                            Message response_msg = databaseResponseQueue.Receive();
                            Json_Obj tempJsonObj = DeserializeRequest(response_msg.Body.ToString());
                            string result;
                            if (tempJsonObj.Status == Json_Obj.RequestStatus.Success && CheckHash(response_msg, data,out result))
                            {


                                
                            }
                            else 
                            {
                                tempJsonObj.Message = $"Error: {tempJsonObj.Status}";
                                // Get bytes from data (string)
                                byte[] msg = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(tempJsonObj));
                                //sends message to client
                                stream.Write(msg, 0, msg.Length);
                            }
                            success = true;
                        }
                    }
                    break;
                case Json_Obj.RequestTypes.Create_User:
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
