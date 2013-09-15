using System;
using System.Transactions;
using EventStore;
using EventStore.Dispatcher;
using EventStore.Persistence.SqlPersistence.SqlDialects;
using Hermes;
using Hermes.Configuration;
using Hermes.Core;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging;

namespace MyDomain.Shell
{
    public static class EventStore
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(EventStore));

        public static IStoreEvents WireupEventStore()
        {
            return Wireup.Init()
                         .UsingInMemoryPersistence()
                         .UsingSqlPersistence("EventStore") // Connection string is in app.config
                         .WithDialect(new MsSqlDialect())
                         .InitializeStorageEngine()
                         .EnlistInAmbientTransaction()
                         .UsingJsonSerialization()
                         .LogToOutputWindow()
                         .UsingSynchronousDispatchScheduler()
                         .DispatchTo(new DelegateMessageDispatcher(DispatchCommit))
                         .Build();
        }

        private static void DispatchCommit(Commit commit)
        {
            using (var scope = TransactionScopeUtils.Begin(TransactionScopeOption.Required, TimeSpan.FromSeconds(30)))
            {
                var bus = ServiceLocator.Current.GetService<IMessageBus>();

                foreach (EventMessage @event in commit.Events)
                {
                    Logger.Info("Dispatching event {0} from commit {1}", @event.GetType().Name, commit.CommitId);
                    bus.Publish(@event.Body);
                }

                TestError.Throw();
                scope.Complete();
            }
        }
    }
}