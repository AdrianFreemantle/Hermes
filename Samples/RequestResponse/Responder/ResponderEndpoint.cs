using System;

using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;

using RequestResponseMessages;

namespace Responder
{
    public class ResponderEndpoint : WorkerEndpoint<AutofacAdapter>
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True";

        protected override void ConfigureEndpoint(IConfigureWorker configuration)
        {
            configuration
                .UseDistributedTransaction()
                .UseJsonSerialization()
                .FirstLevelRetryPolicy(3, TimeSpan.FromSeconds(1))
                .UseSqlTransport(ConnectionString)
                .DefineMessageAs(IsMessage)
                .UseSqlStorage(ConnectionString)                
                .RegisterMessageRoute<AdditionResult>(Address.Parse("Requestor"));
        }

        private static bool IsMessage(Type type)
        {
            return typeof(IMessage).IsAssignableFrom(type) && type.Namespace.StartsWith("RequestResponseMessages");
        }
    }
}
