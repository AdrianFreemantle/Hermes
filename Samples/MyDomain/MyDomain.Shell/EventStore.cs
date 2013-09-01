using System.Transactions;
using EventStore;
using EventStore.Dispatcher;
using EventStore.Persistence.SqlPersistence.SqlDialects;
using Hermes;
using Hermes.Logging;

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
            TestError.Throw();

            using (var scope = TransactionScopeUtils.Begin(TransactionScopeOption.Required))
            {
                foreach (EventMessage @event in commit.Events)
                {
                    Logger.Info("Dispatching event {0} from commit {1}", @event.GetType().Name, commit.CommitId);
                    DomainEvent.Current.Raise(@event.Body);
                }

                TestError.Throw();

                scope.Complete();

                TestError.Throw();
            }
        }
    }
}