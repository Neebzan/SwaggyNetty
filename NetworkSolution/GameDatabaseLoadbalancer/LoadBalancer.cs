using GlobalVariablesLib;
using MSMQHelperUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TcpHelper;

namespace GameDatabaseLoadbalancer
{

    public class LoadBalancer
    {
        TcpListener listener;

        List<PlayerDataModel> slaveOneRequests = new List<PlayerDataModel>();
        List<PlayerDataModel> slaveTwoRequests = new List<PlayerDataModel>();

        List<TcpClient> clients = new List<TcpClient>();

        MessageQueue databaseConsumerQueue;
        MessageQueue databaseProducerQueue;


        public LoadBalancer()
        {
            databaseConsumerQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.MYSQL_PLAYER_DB_CONSUMER_QUEUE_NAME);
            databaseProducerQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.MYSQL_PLAYER_DB_PRODUCER_QUEUE_NAME);

            databaseProducerQueue.ReceiveCompleted += DatabaseProducerQueue_ReceiveCompleted;

            listener = new TcpListener(IPAddress.Any, GlobalVariables.GAME_DATABASE_LOADBALANCER_PORT);
            listener.Start();

            Thread t = new Thread(() => ListenForConnections());
            t.IsBackground = true;
            t.Start();


        }

        private void DatabaseProducerQueue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mQ = (MessageQueue)sender;
            Message m = mQ.EndReceive(e.AsyncResult);
            m.Formatter = new JsonMessageFormatter();

            try
            {
                PlayerDataModel model = JsonConvert.DeserializeObject<PlayerDataModel>(m.Body.ToString());
                //Fjern fra request listen
                if (model.ReadSlaveNumber == 1)
                {
                    lock (slaveOneRequests)
                    {
                        slaveOneRequests.RemoveAll(x => x.UserID == model.UserID && x.RequestTime == model.RequestTime);
                    }
                }
                else
                {
                    lock (slaveTwoRequests)
                    {
                        slaveTwoRequests.RemoveAll(x => x.UserID == model.UserID && x.RequestTime == model.RequestTime);
                    }
                }

                //Vidresend model til serveren
                if (model.ResponseExpected)
                {
                    byte[] data = MessageFormatter.MessageBytes(model);
                    model.RequesterClient.GetStream().Write(data, 0, data.Length);
                }

            }
            catch (Exception error)
            {
                //Invalid letter queue here!
                Console.WriteLine(error.Message);
            }
            mQ.BeginReceive();
        }

        void ListenForConnections()
        {

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                lock (clients)
                    clients.Add(client);
                Thread t = new Thread(() => HandleServerRequests(client));
                t.IsBackground = true;
                t.Start();
            }
        }

        void HandleServerRequests(TcpClient client)
        {
            while (MessageFormatter.Connected(client))
            {
                if (client.GetStream().DataAvailable)
                {
                    List<PlayerDataModel> playerModels;
                    string modelsString = MessageFormatter.ReadStreamOnce(client.GetStream());

                    playerModels = JsonConvert.DeserializeObject<List<PlayerDataModel>>(modelsString);
                    Console.WriteLine("Requests received from server");

                    foreach (var model in playerModels)
                    {
                        if (slaveOneRequests.Count <= slaveTwoRequests.Count)
                        {
                            lock (slaveOneRequests)
                            {
                                model.RequestTime = DateTime.Now.Ticks;
                                model.ReadSlaveNumber = 1;
                                model.RequesterClient = client;
                                slaveOneRequests.Add(model);
                            }
                            SendRequest(model);
                        }
                        else
                        {
                            lock (slaveTwoRequests)
                            {
                                model.RequestTime = DateTime.Now.Ticks;
                                model.ReadSlaveNumber = 2;
                                model.RequesterClient = client;
                                slaveTwoRequests.Add(model);
                            }
                            SendRequest(model);
                        }
                    }

                    Console.WriteLine("Requests sent to translator\nCurrent request distribution:\nSlave One:{0} requests\nSlave Two {1} requests", slaveOneRequests.Count, slaveTwoRequests.Count);
                }
            }
            lock (clients)
                clients.Remove(client);
        }

        void SendRequest(PlayerDataModel model)
        {
            Message m = new Message()
            {
                Formatter = new JsonMessageFormatter(),
                Body = model
            };

            MSMQHelper.SendMessage(databaseConsumerQueue, m);
        }
    }
}
