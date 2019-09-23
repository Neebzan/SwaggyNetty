using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using MSMQHelperUtilities;
using Newtonsoft.Json;
using GlobalVariablesLib;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace Beacon
{
    class Program
    {
        static void Main(string[] args)
        {
            Beacon beacon = new Beacon();

            using (MessageQueue outputQueue = MSMQHelper.CreateMessageQueue("BeaconOutputQueue"))
            {
                MSMQHelper.SendMessage(outputQueue, JsonConvert.SerializeObject(beacon.serverData));
            }

            using (MessageQueue outputQueue = MSMQHelper.CreateMessageQueue("BeaconOutputQueue"))
            {
                ServersData temp = MSMQHelper.GetMessageBody<ServersData>(MSMQHelper.ReceiveMessage(outputQueue));
            }
            Console.ReadKey();
        }


        void KeepOutputQueueUpdated()
        {
            using (MessageQueue outputQueue = MSMQHelper.CreateMessageQueue("BeaconOutputQueue"))
            {
                while (true)
                {
                    //MSMQHelper.SendMessage(outputQueue, JsonConvert.SerializeObject(beacon.ServerData));
                }
            }
        }
    }
}


////ResponseQueue test
//MessageQueue mq = MSMQHelper.CreateMessageQueue("ResponseTest");

//UserModel testModel = new UserModel { ClientSocket = "test", UserID = "TestID" };
////Message msg = new Message()
////{
////    Label = TokenRequestType.CreateToken.ToString(),
////    Formatter = new JsonMessageFormatter(),
////    Body = JsonConvert.SerializeObject(testModel),
////    ResponseQueue = mq                
////};

//MessageQueue inputMq = MSMQHelper.CreateMessageQueue(GlobalVariables.TOKEN_INPUT_QUEUE_NAME);
////MSMQHelper.SendMessage(inputMq, testModel, TokenRequestType.CreateToken.ToString(), mq);

//Message myCreateMsg = MSMQHelper.GenerateMessage(testModel, TokenRequestType.CreateToken.ToString(), mq);
//string response = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJDb25uZWN0aW9uTW9kZWwiOiJ7XCJVc2VySURcIjpcIk15IElEXCIsXCJDbGllbnRTb2NrZXRcIjpudWxsLFwiU2VydmVySURcIjpcIlRlc3Qgc2VydmVyIElEXCIsXCJTZXJ2ZXJTb2NrZXRcIjpudWxsLFwiQ2hhdFNvY2tldFwiOm51bGx9IiwibmJmIjoxNTY5MjMxMjY0LCJleHAiOjE1Njk2NjMyNjQsImlhdCI6MTU2OTIzMTI2NH0._hnyB7A8LHutwa2cyESHdTeP-p_NOe71tUelx4M4JhU";
//Message myVerifyMsg = MSMQHelper.GenerateMessage(response, TokenRequestType.VerifyToken.ToString(), mq);

//MSMQHelper.SendMessage(inputMq, myVerifyMsg);

////inputMq.Send(msg);