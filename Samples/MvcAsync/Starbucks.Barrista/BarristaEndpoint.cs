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
                .DefineMessageAs(IsMessage)
                .NumberOfWorkers(4);
        }

        private static bool IsCommand(Type type)
        {
            return typeof (ICommand).IsAssignableFrom(type) && type.Namespace.StartsWith("Starbucks");
        }

        private static bool IsMessage(Type type)
        {
            return typeof (IMessage).IsAssignableFrom(type) && type.Namespace.StartsWith("Starbucks");
        }
    }
}
