using System;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.Msmq;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;
using MsmqTest.Contracts;

namespace MsmqTest.Endpoint
{
    public class Endpoint : WorkerEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureWorker configuration)
        {
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Info;

            configuration
                .DisableHeartbeatService()
                .DisableMessageAudit()
                .DisablePerformanceMonitoring()
                .UseJsonSerialization()
                .DefineCommandAs(IsCommand)
                .DefineEventAs(IsEvent)
                .NumberOfWorkers(4)
                .EnableCommandValidators()
                .UserNameResolver(ResolveUserName)
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

    public class PrintDateTimeHandler : IHandleMessage<PrintNumber>
    {
        private static readonly ILog Logger = LogFactory.Build<PrintDateTimeHandler>();

        public void Handle(PrintNumber m)
        {
            Logger.Info(m.Number.ToString());
        }
    }
}
