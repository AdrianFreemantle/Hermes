using System;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using Hermes.Storage.SqlServer;
using Hermes.Transports.SqlServer;
using Starbucks.Messages;

namespace Starbucks
{
    public class RequestorEndpoint : ClientEndpoint<AutofacAdapter>
    {
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=MessageBroker;Integrated Security=True";

        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            configuration
                .UseJsonSerialization()
                .UseSqlTransport(ConnectionString)
                .UseSqlStorage(ConnectionString)
                .DefineCommandAs(IsCommand)
                .RegisterMessageRoute<BuyCoffee>(Address.Parse("Starbucks.Barrista"));
        }

        private static bool IsCommand(Type type)
        {
            return typeof(ICommand).IsAssignableFrom(type) && type.Namespace.StartsWith("Starbucks");
        }
    }
}