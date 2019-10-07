using GlobalVariablesLib;
using MSMQHelperUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MySQL_PlayerData_Translator {
        public class InputRecievedEventArgs : EventArgs {
            public PlayerDataRequest RequestType { get; set; }
            public PlayerDataModel Data{ get; set; }
        }
    class MSMQHandler {
            public MessageQueue consumerQueue;
            public MessageQueue producerQueue;

            /// <summary>
            /// Raised when inputs have been recieved and handled
            /// </summary>
            public event EventHandler<InputRecievedEventArgs> NewInputRecieved;

            public MSMQHandler () {
                SetupQueues();
                consumerQueue.BeginReceive();
                //consumerQueue.Formatter = new XmlMessageFormatter(new Type [ ] { typeof(string) });
                consumerQueue.ReceiveCompleted += OnConsumerInputRecieved;
            }

            /// <summary>
            /// Instantiates the message queues
            /// </summary>
            void SetupQueues () {
                consumerQueue = MSMQHelper.CreateMessageQueue(GlobalVariablesLib.GlobalVariables.MYSQL_PLAYER_DB_CONSUMER_QUEUE_NAME);
                producerQueue = MSMQHelper.CreateMessageQueue(GlobalVariablesLib.GlobalVariables.MYSQL_PLAYER_DB_PRODUCER_QUEUE_NAME);
            }

            /// <summary>
            /// Handles messages recieved from the consumer queue
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void OnConsumerInputRecieved (object sender, ReceiveCompletedEventArgs e) {
                MessageQueue mQ = (MessageQueue)sender;
                Message m = mQ.EndReceive(e.AsyncResult);
                m.Formatter = new JsonMessageFormatter();

                try {
                    PlayerDataModel data = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerDataModel>(m.Body.ToString());

                    EventHandler<InputRecievedEventArgs> handler = NewInputRecieved;
                    Task.Factory.StartNew(() => handler?.Invoke(this, new InputRecievedEventArgs() { Data = data, RequestType = data.PlayerDataRequest }));
                }
                catch (Exception eM) {
                    ConsoleFormatter.WriteLineWithTimestamp(eM.Message);
                }
                mQ.BeginReceive();
            }

            /// <summary>
            /// Pushes a message to the producer queue
            /// </summary>
            /// <param name="_user"></param>
            public void EnqueueProducerQueue (PlayerDataModel data) {
                Message msg = new Message(Newtonsoft.Json.JsonConvert.SerializeObject(data));
                msg.Label = data.UserID;
                msg.Formatter = new JsonMessageFormatter();
                MSMQHelper.SendMessage(producerQueue, msg);
            }
        }
    }

