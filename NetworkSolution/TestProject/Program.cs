using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSMQHelperUtilities;
using GlobalVariablesLib;
using System.Messaging;
using JWTlib;

namespace TestProject
{
    class Program
    {
        static MessageQueue userInputMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.TOKEN_INPUT_QUEUE_NAME);
        static MessageQueue beaconInputMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.BEACON_INPUT_QUEUE_NAME);
        static MessageQueue beaconResponseMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.BEACON_RESPONSE_QUEUE_NAME);
        static MessageQueue testMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.TEST_QUEUE_NAME);


        static void Main(string[] args)
        {
            ConsoleKey key = Console.ReadKey().Key;
            Console.WriteLine("1. Request new JWT Token");

            switch(key)
            {
                case ConsoleKey.D1:
                    {
                        RequestJWTToken();
                        break;
                    }
            }

            Console.ReadKey();
        }

        static void RequestJWTToken()
        {
            Console.WriteLine("Username:");
            string username = Console.ReadLine();
            UserModel userModel = new UserModel() { UserID = username };

            Console.WriteLine("Sending request to token system");
            MSMQHelper.SendMessage(userInputMQ, userModel, TokenRequestType.CreateToken.ToString(), testMQ);

            Console.WriteLine("Awaiting response in testMQ");
            Message msg = MSMQHelper.ReceiveMessage(testMQ);

            //JWTPayload payload = JWTManager.GetModelFromToken<JWTPayload>(msg.Body);


            JWTPayload payload = MSMQHelper.GetMessageBody<JWTPayload>(msg);


            Console.WriteLine("Response received");
            Console.WriteLine("Printing payload:");
            Console.WriteLine("UserID = " + payload.User.UserID);
            Console.WriteLine("Servers:");
            foreach (var item in payload.ServersInfo.Servers)
            {
                Console.WriteLine(item.IP + ":" + item.Port);
            }

        }
    }
}
