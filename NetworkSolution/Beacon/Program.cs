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
        static Beacon beacon;

        static void Main(string[] args)
        {
            beacon = new Beacon();

            MessageQueue beaconInputMQ = MSMQHelper.CreateMessageQueue(GlobalVariables.BEACON_INPUT_QUEUE_NAME);

            beaconInputMQ.ReceiveCompleted += BeaconInputReaceived;
            beaconInputMQ.BeginReceive();

            Console.ReadKey();
        }

        private static void BeaconInputReaceived(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mQ = (MessageQueue)sender;
            Message m = mQ.EndReceive(e.AsyncResult);

            MSMQHelper.SendMessage(m.ResponseQueue, beacon.GetServerData(), "ServerData");
            Console.WriteLine("Server information sent to {0}", m.ResponseQueue.QueueName);

            mQ.BeginReceive();
        }
    }
}