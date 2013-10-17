using System;
using System.Threading;
using Hermes;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;
using Starbucks.Messages;

namespace Starbucks.Barrista
{
    public class BarristaEndpoint : WorkerEndpoint<AutofacAdapter>
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True";

        protected override void ConfigureEndpoint(IConfigureWorker configuration)
        {           
            configuration
                .SecondLevelRetryPolicy(3, TimeSpan.FromSeconds(10))
                .FirstLevelRetryPolicy(1, TimeSpan.FromMilliseconds(10))                
                .UseJsonSerialization()                
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .DefineCommandAs(IsCommand)
                .NumberOfWorkers(4);
        }

        private static bool IsCommand(Type type)
        {
            return typeof(ICommand).IsAssignableFrom(type) && type.Namespace.StartsWith("Starbucks");
        }
    }

    public class MyService : IService
    {
        public void Dispose()
        {
            
        }

        public void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Console.WriteLine(DateTime.Now);
                Thread.Sleep(1000);
            }
        }
    }


    public class MyUnsafeService : IService
    {
        public void Dispose()
        {

        }

        public void Run(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(1000);
                Console.WriteLine("Tick tick tick....");

                if (DateTime.Now.Second % 10 == 0)
                {
                    Console.WriteLine("Boom!");
                    throw new Exception("Unhandled");
                }
            }
        }
    }
}
