using System;
using System.Linq;
using Hermes.EntityFramework;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
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
        private static readonly Guid SessionId = Guid.NewGuid();

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
            var recordLogs = repositoryFactory.GetRepository<RecordLog>();
            
            recordLogs.Add(new RecordLog { RecordId = message.RecordId });
        }

        public void Handle(IRecordAddedToDatabase_V2 message)
        {
            var recordCounts = repositoryFactory.GetRepository<RecordCount>();

            var count = recordCounts.FirstOrDefault(recordCount => recordCount.Id == SessionId);

            if (count == null)
            {
                count = new RecordCount { Id = SessionId };
                recordCounts.Add(count);
            }

            count.NumberOfRecords++;
        }
    }

    public class RecordCountWorker : ScheduledWorkerService
    {
        public RecordCountWorker()
        {
            this.RunImmediatelyOnStartup = false;
            this.SetSchedule(TimeSpan.FromSeconds(10));
        }

        protected override void DoWork()
        {
            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                var repositoryFactory = scope.GetInstance<IRepositoryFactory>();
                var recordCounts = repositoryFactory.GetRepository<RecordCount>();

                var record = recordCounts.First();

                Logger.Fatal("Current number of records : {0}", record.NumberOfRecords); //logged as fatal to so it will show even when the logging filter is set to fatal.
            }
        }
    }
}
