using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSMQHelperUtilities;
using GlobalVariablesLib;
using System.Messaging;
using JWTlib;
using TcpHelper;
using System.Net.Sockets;

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
            Console.WriteLine("1. Request new JWT Token");
            while (true)
            {
                ConsoleKey key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.D1:
                        {
                            RequestJWTToken();
                            break;
                        }
                    case ConsoleKey.D2:
                        {
                            SpoofDataModel();
                            break;
                        }
                }

            }
        }

        static void SpoofDataModel()
        {
            List<PlayerDataModel> meh = new List<PlayerDataModel>();

            for (int i = 0; i < 1000; i++)
            {
                meh.Add(new PlayerDataModel()
                {
                    PlayerDataRequest = PlayerDataRequest.Read,
                    UserID = "YeeHaw",
                    ResponseExpected = false
                    //Online = true,
                    //PositionX = 5,
                    //PositionY = 10,
                    //ResponseExpected = false
                });
            }


            byte[] data = MessageFormatter.MessageBytes(meh);
            TcpClient client = new TcpClient("127.0.0.1", GlobalVariables.GAME_DATABASE_LOADBALANCER_PORT);
            client.GetStream().Write(data, 0, data.Length);

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
            Console.WriteLine("UserID = " + payload.UserID);
            Console.WriteLine("Servers:");
            foreach (var item in payload.ServersInfo.Servers)
            {
                Console.WriteLine(item.IP + ":" + item.Port);
            }

        }
    }
}
