using System;
using System.Threading;
using Hermes;
using Hermes.EntityFramework;
using Hermes.Messaging;
using IntegrationTest.Contracts;

using IntegrationTests.PersistenceModel;

namespace IntegrationTest.Endpoint
{
    public class Handler 
        : IHandleMessage<AddRecordToDatabase>
        , IHandleMessage<IRecordAddedToDatabase>
        , IHandleMessage<IRecordAddedToDatabase_V2>
    {
        private readonly IRepositoryFactory repositoryFactory;
        private readonly IMessageBus messageBus;

        public Handler(IRepositoryFactory repositoryFactory, IMessageBus messageBus)
        {
            this.repositoryFactory = repositoryFactory;
            this.messageBus = messageBus;
        }

        public void Handle(AddRecordToDatabase message)
        {
            var repository = repositoryFactory.GetRepository<Record>();

            repository.Add(new Record
                {
                    Id = message.RecordId, 
                    RecordNumber = message.RecordNumber
                });

            messageBus.Publish(message.RecordId, new RecordAddedToDatabase(message.RecordId));
        }

        public void Handle(IRecordAddedToDatabase message)
        {
            var repository = repositoryFactory.GetRepository<RecordLog>();

            repository.Add(new RecordLog { RecordId = message.RecordId });
        }

        public void Handle(IRecordAddedToDatabase_V2 message)
        {
        }
    }
}
