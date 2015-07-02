using System;
using System.Linq;
using EntityFrameworkTest.Model;
using EntityFrameworkTest.Queries.EmployeeDtoQueries;
using Hermes.EntityFramework.Queries;
using Hermes.EntityFramework.Queues;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;

namespace EntityFrameworkTest
{
    class Program
    {
        static void Main(string[] args)
        {
            LogFactory.BuildLogger = type => new ConsoleWindowLogger(type);
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Info;

            var endpoint = new Endpoint();
            endpoint.Start();

            using (var scope = Settings.RootContainer.BeginLifetimeScope())
            {
                var queryService = scope.GetInstance<DatabaseQuery>();

                var companies = queryService.GetQueryable<Company>();

                var c = companies.ToArray();
            }
        }
    }

    public class QueueHandler : IHandleMessage<IntitalizeQueue>
    {
        private readonly QueueFactory queueFactory;
        private readonly QueueStore queueStore;


        public QueueHandler(QueueFactory queueFactory, QueueStore queueStore)
        {
            this.queueFactory = queueFactory;
            this.queueStore = queueStore;
        }

        public void Handle(IntitalizeQueue m)
        {
            queueFactory.CreateQueueIfNecessary(m.Name);

            Guid id = Guid.NewGuid();
            
            queueStore.Enqueue("Test", id);
            var peekId = queueStore.Peek("Test");
            var dequeueId = queueStore.Deque("Test");
            var empty = queueStore.Peek("Test");


        }
    }
}
