﻿using System.IO;
using System.Text;
using System.Transactions;
using Hermes.Compression;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Serialization;

namespace Hermes.EntityFramework.MessageStore
{
    public class LocalEventStore
    {
        
    }

    public class LocalMessageStore : IStoreLocalMessages
    {
        public void SaveSession(LocalSession session)
        {
            using (var transactionScope = TransactionScopeUtils.Begin(TransactionScopeOption.Suppress))
            using (var lifetimeScope = Settings.RootContainer.BeginLifetimeScope())
            {
                var unitOfWork = lifetimeScope.GetInstance<EntityFrameworkUnitOfWork>();
                var serializer = lifetimeScope.GetInstance<ISerializeObjects>();

                var messageStore = unitOfWork.GetRepository<MessageStore>();

                messageStore.Add(new MessageStore
                {
                    Headers = serializer.SerializeObject(session.Headers),
                    Message = CompressMessage(serializer, session.Message),
                    MessageId = session.MessageId,
                    Failed = session.Headers.ContainsKey(HeaderKeys.FailureDetails)
                });

                unitOfWork.Commit();
                transactionScope.Complete();
            }
        }

        private static byte[] CompressMessage(ISerializeObjects serializer, object message)
        {
            var serializedMessage = serializer.SerializeObject(message);
            byte[] bytes = Encoding.UTF8.GetBytes(serializedMessage);

            using (var stream = new MemoryStream(bytes))
            {
                bytes = GzipCompressor.Compress(stream);
            }

            return bytes;
        }
    }
}
