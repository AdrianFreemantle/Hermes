using System;
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
            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.NoLogging;

            configuration
                .SecondLevelRetryPolicy(10, TimeSpan.FromSeconds(10))
                .UseJsonSerialization()
                .UseSqlTransport()
                .ConfigureEntityFramework<IntegrationTestContext>("IntegrationTest")
                .DefineCommandAs(IsCommand)
                .DefineEventAs(IsEvent)
                .NumberOfWorkers(16)
                .FlushQueueOnStartup(true);
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

    public class Handler 
        : IHandleMessage<AddRecordToDatabase>
        , IHandleMessage<RecordAddedToDatabase>
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
            System.Threading.Thread.Sleep(10);
            //var repository = repositoryFactory.GetRepository<Record>();
            //repository.Add(new Record { Id = message.RecordId });
            messageBus.Publish(new RecordAddedToDatabase(message.RecordId));
        }

        public void Handle(RecordAddedToDatabase message)
        {
            System.Threading.Thread.Sleep(10);
            //var repository = repositoryFactory.GetRepository<RecordLog>();
            //repository.Add(new RecordLog { RecordId = message.RecordId });
        }
    }
}
