using MSMQHelperUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MySQL_translator
{

    public class InputRecievedEventArgs : EventArgs
    {
        public GlobalVariablesLib.RequestTypes requestType { get; set; }
        public User User { get; set; }
    }

    public class MessageQueueHandler
    {
        public MessageQueue consumerQueue;
        public MessageQueue producerQueue;

        /// <summary>
        /// Raised when inputs have been recieved and handled
        /// </summary>
        public event EventHandler<InputRecievedEventArgs> NewInputRecieved;

        public MessageQueueHandler () {
            SetupQueues();
            consumerQueue.BeginReceive();
            consumerQueue.Formatter = new XmlMessageFormatter(new Type [ ] { typeof(string) });
            consumerQueue.ReceiveCompleted += OnConsumerInputRecieved;
        }

        /// <summary>
        /// Instantiates the message queues
        /// </summary>
        void SetupQueues () {
            consumerQueue = MSMQHelperUtilities.MSMQHelper.CreateMessageQueue(GlobalVariablesLib.GlobalVariables.CONSUMER_QUEUE_NAME);
            producerQueue = MSMQHelperUtilities.MSMQHelper.CreateMessageQueue(GlobalVariablesLib.GlobalVariables.PRODUCER_QUEUE_NAME);
        }

        /// <summary>
        /// Handles messages recieved from the consumer queue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConsumerInputRecieved (object sender, ReceiveCompletedEventArgs e) {
            MessageQueue mQ = (MessageQueue)sender;
            Message m = mQ.EndReceive(e.AsyncResult);
            Console.WriteLine("Message recieved: " + m.Body);

            try {
                User user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(m.Body.ToString());
                GlobalVariablesLib.RequestTypes requestType = GlobalVariablesLib.RequestTypes.Get_User;

                switch (user.RequestType) {
                    case GlobalVariablesLib.RequestTypes.Get_User:
                        requestType = GlobalVariablesLib.RequestTypes.Get_User;
                        break;
                    case GlobalVariablesLib.RequestTypes.Create_User:
                        requestType = GlobalVariablesLib.RequestTypes.Create_User;
                        break;
                    default:
                        break;
                }

                EventHandler<InputRecievedEventArgs> handler = NewInputRecieved;
                handler?.Invoke(this, new InputRecievedEventArgs() { User = user, requestType = requestType });
            }
            catch (Exception eM) {
                Console.WriteLine(eM.Message);
            }
            mQ.BeginReceive();
        }

        /// <summary>
        /// Pushes a message to the producer queue
        /// </summary>
        /// <param name="_user"></param>
        public void PushProducerQueue (User _user) {
            Message msg = new Message(Newtonsoft.Json.JsonConvert.SerializeObject(_user));
            msg.Label = _user.UserID;
            MSMQHelperUtilities.MSMQHelper.SendMessage(producerQueue, msg);
        }
    }
}
