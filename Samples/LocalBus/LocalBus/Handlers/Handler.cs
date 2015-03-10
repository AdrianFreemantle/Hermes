using System;
using Contracts;
using Hermes;
using Hermes.EntityFramework;
using Hermes.Messaging;
using LocalBus.Contracts;
using LocalBus.Persistence;

namespace LocalBus.Handlers
{
    public class Handler :
        IHandleMessage<AddRecordToDatabase>,
        IHandleMessage<IRecordAddedToDatabase>,
        IHandleMessage<IRecordAddedToDatabase_V2>,
        IHandleMessage<RecordAddedToDatabase>,
        IValidateCommand<AddRecordToDatabase>
    {
        private readonly IRepositoryFactory repositoryFactory;
        private readonly IInMemoryEventBus localEventBus;
        private readonly IMessageBus messageBus;

        public Handler(IRepositoryFactory repositoryFactory, IInMemoryEventBus localEventBus, IMessageBus messageBus)
        {
            this.repositoryFactory = repositoryFactory;
            this.localEventBus = localEventBus;
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

            localEventBus.Raise(new RecordAddedToDatabase(message.RecordId, message.RecordNumber));
        }

        public void Handle(IRecordAddedToDatabase message)
        {
            var repository = repositoryFactory.GetRepository<RecordLog>();
          
            repository.Add(new RecordLog
            {
                RecordId = message.RecordId
            });
        }

        public void Handle(IRecordAddedToDatabase_V2 message)
        {
            if (message.RecordNumber % 100 == 0)
            {
                messageBus.Publish(new ErrorOccured("This should never be delivered"));
                throw new HermesTestingException();
            }
        }

        public void Validate(AddRecordToDatabase command)
        {
            SequentialGuid.ValidateSequentialGuid(command.RecordId);
        }

        public void Handle(RecordAddedToDatabase m)
        {
            Console.WriteLine("blah");
        }
    }
}
