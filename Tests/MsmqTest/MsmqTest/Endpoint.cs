using System;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.Msmq;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using MsmqTest.Contracts;

namespace MsmqTest
{
    public class Endpoint : LocalEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Debug;

            configuration
                .UseJsonSerialization()
                .DefineCommandAs(IsCommand)
                .DefineEventAs(IsEvent)
                .NumberOfWorkers(1)
                .EnableCommandValidators()
                .UserNameResolver(ResolveUserName)
                .RegisterMessageRoute<PrintNumber>(Address.Parse(String.Format("MsmqTest@{0}", Environment.MachineName)))
                .UseMsmqTransport();
        }

        private string ResolveUserName()
        {
            var bus = Settings.RootContainer.GetInstance<IMessageBus>();

            if (String.IsNullOrWhiteSpace(bus.CurrentMessage.UserName))
            {
                return "error";
            }

            return bus.CurrentMessage.UserName;
        }

        private static bool IsCommand(Type type)
        {
            if (type == null || type.Namespace == null)
                return false;

            return typeof(ICommand).IsAssignableFrom(type);
        }

        private static bool IsEvent(Type type)
        {
            if (type == null || type.Namespace == null)
                return false;

            return typeof(IEvent).IsAssignableFrom(type) || typeof(IDomainEvent).IsAssignableFrom(type);
        }
    }
}