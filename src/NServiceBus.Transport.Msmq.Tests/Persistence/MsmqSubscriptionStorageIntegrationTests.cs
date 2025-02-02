﻿namespace NServiceBus.Transport.Msmq.Tests.Persistence
{
    using MSMQ.Messaging;
    using System.Threading.Tasks;
    using Extensibility;
    using NServiceBus.Persistence.Msmq;
    using NUnit.Framework;
    using Unicast.Subscriptions.MessageDrivenSubscriptions;
    using MessageType = Unicast.Subscriptions.MessageType;

    public class MsmqSubscriptionStorageIntegrationTests
    {
        const string TestQueueName = "NServiceBus.Core.Tests.MsmqSubscriptionStorageIntegrationTests";

        [SetUp]
        public void Setup()
        {
            DeleteQueueIfPresent(TestQueueName);
        }

        [TearDown]
        public void TearDown()
        {
            DeleteQueueIfPresent(TestQueueName);
        }

        [Test]
        public async Task ShouldRemoveSubscriptionsInTransactionalMode()
        {
            var address = MsmqAddress.Parse(TestQueueName);
            var queuePath = address.PathWithoutPrefix;

            MessageQueue.Create(queuePath, true);

            using (var queue = new MessageQueue(queuePath))
            {
                queue.Send(new Message
                {
                    Label = "subscriber",
                    Body = typeof(MyMessage).AssemblyQualifiedName
                }, MessageQueueTransactionType.Single);
            }

            var storage = new MsmqSubscriptionStorage(new MsmqSubscriptionStorageQueue(address, true));

            await storage.Unsubscribe(new Subscriber("subscriber", "subscriber"), new MessageType(typeof(MyMessage)), new ContextBag());

            using (var queue = new MessageQueue(queuePath))
            {
                CollectionAssert.IsEmpty(queue.GetAllMessages());
            }
        }

        [Test]
        public async Task ShouldRemoveSubscriptionsInNonTransactionalMode()
        {
            var address = MsmqAddress.Parse(TestQueueName);
            var queuePath = address.PathWithoutPrefix;

            MessageQueue.Create(queuePath, false);

            using (var queue = new MessageQueue(queuePath))
            {
                queue.Send(new Message
                {
                    Label = "subscriber",
                    Body = typeof(MyMessage).AssemblyQualifiedName
                }, MessageQueueTransactionType.None);
            }

            var storage = new MsmqSubscriptionStorage(new MsmqSubscriptionStorageQueue(address, false));

            await storage.Unsubscribe(new Subscriber("subscriber", "subscriber"), new MessageType(typeof(MyMessage)), new ContextBag());

            using (var queue = new MessageQueue(queuePath))
            {
                CollectionAssert.IsEmpty(queue.GetAllMessages());
            }
        }

        void DeleteQueueIfPresent(string queueName)
        {
            var path = MsmqAddress.Parse(queueName).PathWithoutPrefix;

            if (MessageQueue.Exists(path))
            {
                MessageQueue.Delete(path);
            }
        }

        class MyMessage
        {
        }
    }
}