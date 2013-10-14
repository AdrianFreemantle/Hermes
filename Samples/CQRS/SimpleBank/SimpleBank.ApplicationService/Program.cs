using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading;

using Hermes.EntityFramework;
using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.EntityFramework;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using SimpleBank.DataModel;
using SimpleBank.DataModel.ReadModel;

namespace SimpleBank.ApplicationService
{
    class Program
    {
        //private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True";
        private const string ConnectionString = "data source=CG-T-SQL-03V;Initial Catalog=CG_T_DB_MSGBRKR;Persist Security Info=True;User ID=CG_T_USR_SYNAFreemantle;password=vimes Sep01";

        private static void Main(string[] args)
        {
            ConsoleWindowLogger.MinimumLogLevel = ConsoleWindowLogger.LogLevel.Info;

            var config = Configure
                .ServerEndpoint("SimpleBank.ApplicationService", new AutofacAdapter())
                .UseConsoleWindowLogger()
                .UseJsonSerialization()
                .UseUnicastBus()
                .ConfigureEntityFramework<SimpleBankContext>("SimpleBank")
                .UseDistributedTransaction()
                .FirstLevelRetryPolicy(3, TimeSpan.FromMilliseconds(100))
                .SecondLevelRetryPolicy(0, TimeSpan.Zero)
                .UseSqlTransport(ConnectionString)
                .UseEntityFrameworkKeyValueStorage()
                .UseSqlStorage(ConnectionString)
                .ScanForHandlersIn(Assembly.GetExecutingAssembly());

            Settings.Builder.RegisterType<AggregateRepository>(DependencyLifecycle.InstancePerLifetimeScope);
            InitDatabase(Settings.RootContainer);
            config.Start();

            var token = new CancellationTokenSource(TimeSpan.FromHours(2));

            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(50);
            }

            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        /// <summary>
        /// EF creates the database the first time a query is executed against it.
        /// This causes a problem due to the fact that you cant create a database in a transaction.
        /// This code will cause the database to be created as soon as our application starts and 
        /// will do so outside of a transaction.
        /// </summary>
        /// <param name="container"></param>
        private static void InitDatabase(IContainer container)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                var unitOfWork = scope.GetInstance<IUnitOfWork>();
                var repository = unitOfWork.GetRepository<PortfolioRecord>();
                repository.Any();
                Database.SetInitializer(new SimpleBankContextInitializer());
            }

        }
    }
}
