﻿namespace NServiceBus.Transport.Msmq.Tests
{
    using MSMQ.Messaging;

    static class MsmqHelpers
    {
        public static void DeleteQueue(string path)
        {
            if (!MessageQueue.Exists(path))
            {
                return;
            }
            MessageQueue.Delete(path);
        }

        public static void CreateQueue(string path)
        {
            if (MessageQueue.Exists(path))
            {
                return;
            }
            using (MessageQueue.Create(path, true))
            {
            }
        }
    }
}