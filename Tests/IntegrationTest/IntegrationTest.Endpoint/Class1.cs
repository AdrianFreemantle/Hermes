using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Hermes.EntityFramework;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;

using IntegrationTest.Contracts;

using IntegrationTests.PersistenceModel;

namespace IntegrationTest.Endpoint
{
    public class Endpoint : WorkerEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureWorker configuration)
        {
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Debug;

            configuration
                .FirstLevelRetryPolicy(2)
                .SecondLevelRetryPolicy(10, TimeSpan.FromSeconds(10))
                .UseJsonSerialization()
                .UseSqlTransport()
                .DefineCommandAs(IsCommand)
                .DefineEventAs(IsEvent)
                .NumberOfWorkers(4)
                .FlushQueueOnStartup(true)
                .ConfigureEntityFramework<IntegrationTestContext>("IntegrationTest");
        }

        private static bool IsCommand(Type type)
        {
            return typeof(ICommand).IsAssignableFrom(type);
        }

        private static bool IsEvent(Type type)
        {
            return typeof(IEvent).IsAssignableFrom(type);
        }
    }

    public class InitializeDatabase : INeedToInitializeSomething
    {
        public void Initialize()
        {
            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                Database.SetInitializer(new DatabaseInitializer());
                var repositoryFactory = scope.GetInstance<IRepositoryFactory>();
                bool result = repositoryFactory.GetRepository<Record>().Any(); //database gets created with first opperation against it

                if (result)
                {
                    throw new Exception("Expected an empty database");
                }
            }
        }
    }

    public class RecordAddedToDatabase : IRecordAddedToDatabase_V2
    {
        public Guid RecordId { get; private set; }
        public List<Guid> RandomData { get; private set; }

        public RecordAddedToDatabase(Guid recordId)
        {
            RecordId = recordId;

            RandomData = new List<Guid>();

            for (int i = 0; i < 10; i++)
            {
                RandomData.Add(Guid.NewGuid());
            }
        }
    }

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
            var recordRepository = repositoryFactory.GetRepository<Record>();

            var repository = repositoryFactory.GetRepository<RecordLog>();
            repository.Add(new RecordLog { RecordId = message.RecordId });
        }

        public void Handle(IRecordAddedToDatabase_V2 message)
        {
            System.Threading.Thread.Sleep(10);
        }
    }
}
