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

        int tempIndex = 0;



        public LoadBalancer()
        {
            databaseConsumerQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.MYSQL_PLAYER_DB_CONSUMER_QUEUE_NAME);
            databaseProducerQueue = MSMQHelper.CreateMessageQueue(GlobalVariables.MYSQL_PLAYER_DB_PRODUCER_QUEUE_NAME);

            databaseProducerQueue.ReceiveCompleted += DatabaseProducerQueue_ReceiveCompleted;
            databaseProducerQueue.BeginReceive();

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

            Task.Factory.StartNew(() => ProcessMessage(m));


            mQ.BeginReceive();
        }

        void ProcessMessage(Message msg)
        {
            Console.WriteLine("Message received from Producer Queue");
            try
            {
                PlayerDataModel model = JsonConvert.DeserializeObject<PlayerDataModel>(msg.Body.ToString());
                //Fjern fra request listen
                if (model.ReadSlaveNumber == 1)
                {
                    lock (slaveOneRequests)
                    {
                        int count = slaveOneRequests.Count;
                        //slaveOneRequests.RemoveAll(x => x.UserID == model.UserID && x.RequestTime == model.RequestTime);
                        PlayerDataModel temp = new PlayerDataModel();
                        foreach (var item in slaveOneRequests)
                        {
                            if (item.UserID == model.UserID && item.RequestTime == model.RequestTime)
                            {
                                temp = item;
                                model.RequesterClient = item.RequesterClient;
                                break;
                            }
                        }                        
                        slaveOneRequests.Remove(temp);


                        //if (count == slaveOneRequests.Count)
                        //{
                        //    int h = count;
                        //}
                    }
                }
                else if (model.ReadSlaveNumber == 2)
                {
                    lock (slaveTwoRequests)
                    {
                        int count = slaveTwoRequests.Count;
                        //slaveTwoRequests.RemoveAll(x => x.UserID == model.UserID && x.RequestTime == model.RequestTime);
                        PlayerDataModel temp = new PlayerDataModel();
                        foreach (var item in slaveTwoRequests)
                        {
                            if (item.UserID == model.UserID && item.RequestTime == model.RequestTime)
                            {
                                temp = item;
                                model.RequesterClient = item.RequesterClient;
                                break;
                            }
                        }
                        slaveTwoRequests.Remove(temp);


                        //if (count == slaveTwoRequests.Count)
                        //{
                        //    int h = count;
                        //}
                    }
                }
                else
                {
                    throw new Exception("NO MATCH");
                }

                //Vidresend model til serveren
                if (model.ResponseExpected)
                {
                    byte[] data = MessageFormatter.MessageBytes(model);
                    model.RequesterClient.GetStream().Write(data, 0, data.Length);
                    Console.WriteLine("Data sent to client via TCP");
                }
                Console.WriteLine("Message handled!\nCurrent request distribution:\nSlave One:{0} requests\nSlave Two {1} requests", slaveOneRequests.Count, slaveTwoRequests.Count);
            }
            catch (Exception error)
            {
                //Invalid letter queue here!
                Console.WriteLine(error.Message);
            }
        }

        void ListenForConnections()
        {
            Console.WriteLine("Listening for connections");
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                lock (clients)
                    clients.Add(client);
                Thread t = new Thread(() => HandleServerRequests(client));
                t.IsBackground = true;
                t.Start();
                Console.WriteLine("Connection added!");
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


                    //long batchTick = DateTime.Now.Ticks;
                    foreach (var model in playerModels)
                    {
                        if (slaveOneRequests.Count <= slaveTwoRequests.Count)
                        {
                            lock (slaveOneRequests)
                            {
                                model.RequestTime = tempIndex;
                                model.ReadSlaveNumber = 1;
                                model.RequesterClient = client;
                                slaveOneRequests.Add(model);
                                //byte[] data = MessageFormatter.MessageBytes(model);
                                //model.RequesterClient.GetStream().Write(data, 0, data.Length);
                            }
                            SendRequest(model);
                        }
                        else
                        {
                            lock (slaveTwoRequests)
                            {
                                model.RequestTime = tempIndex;
                                model.ReadSlaveNumber = 2;
                                model.RequesterClient = client;
                                slaveTwoRequests.Add(model);
                                //byte[] data = MessageFormatter.MessageBytes(model);
                                //model.RequesterClient.GetStream().Write(data, 0, data.Length);
                            }
                            SendRequest(model);
                        }
                        tempIndex++;
                    }



                    Console.WriteLine("Requests sent to translator\nCurrent request distribution:\nSlave One:{0} requests\nSlave Two {1} requests", slaveOneRequests.Count, slaveTwoRequests.Count);
                }
                Thread.Sleep(16);
            }
            lock (clients)
                clients.Remove(client);
        }

        void SendRequest(PlayerDataModel model)
        {
            Message m = new Message()
            {
                Formatter = new JsonMessageFormatter(),
                Body = JsonConvert.SerializeObject(model)
            };

            MSMQHelper.SendMessage(databaseConsumerQueue, m);
        }
    }
}
