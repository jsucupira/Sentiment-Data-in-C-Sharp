using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Messaging;
using System.Text;
using Core.Common.Helpers;
using Data.Contracts;

namespace Data.MSQueue
{
    [Export(typeof(IStorage))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MsmQueueStorage : IStorage
    {
        public MsmQueueStorage()
        {
            if (MessageQueue.Exists(@".\private$\Twitter"))
                _queue = new MessageQueue(@".\private$\Twitter");
            else
                _queue = MessageQueue.Create(@".\private$\Twitter");
        }

        private MessageQueue _queue;

        public object Add(string data)
        {
            var items = Encoding.UTF8.GetBytes(data);

            foreach (var item in items.GetArrayFromString())
            {
                using (Message message = new Message(item))
                    _queue.Send(message);
            }

            return null;
        }

        public IEnumerable<string> Get(object filteringObject)
        {
            List<string> tweets = new List<string>();
            try
            {
                if (MessageQueue.Exists(@".\private$\Twitter"))
                    _queue = new MessageQueue(@".\private$\Twitter");
                else
                    Console.WriteLine("Queue does not exists.");
                Message message;
                while ((message = Receive(_queue)) != null)
                {
                    message.Formatter = new XmlMessageFormatter(new[] { "System.String" });
                    try
                    {
                        byte[] byteArray = Encoding.UTF8.GetBytes(message.Body.ToString());
                        using (MemoryStream stream = new MemoryStream(byteArray))
                        using (StreamReader reader = new StreamReader(stream))
                            tweets.Add(reader.ReadToEnd());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return tweets;
        }

        private static Message Receive(MessageQueue queue)
        {
            try
            {
                return queue.Receive(TimeSpan.Zero);
            }
            catch (MessageQueueException mqe)
            {
                if (mqe.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
                    return null;
                throw;
            }
        }
    }
}