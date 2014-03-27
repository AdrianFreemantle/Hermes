using Hermes.EntityFramework;
using Hermes.Messaging;
using IntegrationTest.Client.Contracts;
using IntegrationTest.Client.Persistence;

namespace IntegrationTest.Client.Handlers
{
    public class Handler
            : IHandleMessage<AddRecordToDatabase>
            , IHandleMessage<IRecordAddedToDatabase>
            , IHandleMessage<IRecordAddedToDatabase_V2>
    {
        private readonly IRepositoryFactory repositoryFactory;
        private readonly IInMemoryBus messageBus;

        public Handler(IRepositoryFactory repositoryFactory, IInMemoryBus messageBus)
        {
            this.repositoryFactory = repositoryFactory;
            this.messageBus = messageBus;
        }

        public void Handle(AddRecordToDatabase message)
        {
            System.Threading.Thread.Sleep(10);
            var repository = repositoryFactory.GetRepository<Record>();

            repository.Add(
                new Record
                {
                    Id = message.RecordId,
                    RecordNumber = message.RecordNumber
                });

            messageBus.Raise(new RecordAddedToDatabase(message.RecordId));
        }

        public void Handle(IRecordAddedToDatabase message)
        {
            var repository = repositoryFactory.GetRepository<RecordLog>();
            repository.Add(new RecordLog { RecordId = message.RecordId });
        }

        public void Handle(IRecordAddedToDatabase_V2 message)
        {
            System.Threading.Thread.Sleep(10);
        }
    }
}
