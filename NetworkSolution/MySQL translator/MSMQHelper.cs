using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace MSMQHelperUtilities
{
    public static class MSMQHelper
    {
        public static void SendMessage(MessageQueue mQ, Message message)
        {
            mQ.Send(message);
        }

        public static void SendMessage(MessageQueue mQ, object obj)
        {
            mQ.Send(obj);
        }

        public static void SendMessage<T>(MessageQueue mQ, T obj)
        {
            Message msg = new Message()
            {
                Formatter = new JsonMessageFormatter(),
                Label = obj.GetType().Name,
                Body = obj
            };
            mQ.Send(msg);
        }

        public static Message ReceiveMessage(MessageQueue mQ)
        {
            return mQ.Receive();
        }

        public static Message ReceiveMessage(MessageQueue mQ, TimeSpan timeout)
        {
            return mQ.Receive(timeout);
        }

        public static T GetMessageBody<T>(Message message)
        {
            message.Formatter = new JsonMessageFormatter();
            return ReceiveHelper<T>(message.Body);
        }

        private static T ReceiveHelper<T>(object body)
        {
            return JsonConvert.DeserializeObject<T>(body.ToString());
        }

        /// <summary>
        /// LOCAL ONLY - Creates a new MessageQueue if none exists, or returns existing MessageQueue
        /// </summary>
        /// <param name="messageQueueName"></param>
        /// <param name="privateQueue"></param>
        /// <returns></returns>
        public static MessageQueue CreateMessageQueue(string messageQueueName, bool privateQueue = true)
        {
            AccessControlList acl = new AccessControlList();
            Trustee tr = new Trustee("ANONYMOUS LOGON");
            AccessControlEntry entry = new AccessControlEntry(tr, GenericAccessRights.All, StandardAccessRights.All, AccessControlEntryType.Allow);
            acl.Add(entry);


            if (privateQueue)
            {
                if (!MessageQueue.Exists(".\\private$\\" + messageQueueName))
                {
                    // Create the queue if it does not exist.
                    MessageQueue myMQ = MessageQueue.Create(".\\private$\\" + messageQueueName);
                    myMQ.SetPermissions(acl);
                    return myMQ;
                }
                else
                    return new MessageQueue(".\\private$\\" + messageQueueName);

            }
            else
            {
                if (!MessageQueue.Exists(".\\" + messageQueueName))
                {
                    // Create the queue if it does not exist.
                    MessageQueue myMQ = MessageQueue.Create(".\\" + messageQueueName);
                    myMQ.SetPermissions(acl);
                    return myMQ;
                }
                else
                    return new MessageQueue(".\\" + messageQueueName);
            }
        }
    }
}
