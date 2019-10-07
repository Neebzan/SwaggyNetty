using MSMQHelperUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;
using GlobalVariablesLib;

namespace MySQL_translator
{

    public class InputRecievedEventArgs : EventArgs
    {
        public GlobalVariablesLib.RequestTypes RequestType { get; set; }
        public UserModel User { get; set; }
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
            //consumerQueue.Formatter = new XmlMessageFormatter(new Type [ ] { typeof(string) });
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
            m.Formatter = new JsonMessageFormatter();

            try {
                UserModel user = Newtonsoft.Json.JsonConvert.DeserializeObject<UserModel>(m.Body.ToString());

                EventHandler<InputRecievedEventArgs> handler = NewInputRecieved;
                Task.Factory.StartNew(() => handler?.Invoke(this, new InputRecievedEventArgs() { User = user, RequestType = user.RequestType }));
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
        public void EnqueueProducerQueue (UserModel _user) {
            Message msg = new Message(Newtonsoft.Json.JsonConvert.SerializeObject(_user));
            msg.Label = _user.UserID;
            msg.Formatter = new JsonMessageFormatter();
            MSMQHelper.SendMessage(producerQueue, msg);
        }
    }
}
