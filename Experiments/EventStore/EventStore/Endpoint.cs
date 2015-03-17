using System;
using Hermes.EntityFramework;
using Hermes.Logging;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;

namespace EventStore
{
    public class Endpoint : LocalEndpoint<AutofacAdapter>
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(Endpoint));

        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            ConsoleWindowLogger.MinimumLogLevel = LogLevel.Info;

            configuration
                .DisableDistributedTransactions()
                .EnableCommandValidators()
                .UseJsonSerialization()
                .DefineEventAs(IsEvent)
                .DefineCommandAs(IsCommand)
                .RegisterDependencies<EventStoreDependencyRegistrar>()
                .NumberOfWorkers(1)
                .UserNameResolver(ResolveUserName)
                .UseSqlTransport("SqlTransport")
                .ConfigureEntityFramework<EventStoreContext>("EventStore");
        }

        private string ResolveUserName()
        {
            Logger.Debug("Resolving User Name");
            var bus = Settings.RootContainer.GetInstance<IMessageBus>();

            if (String.IsNullOrWhiteSpace(bus.CurrentMessage.UserName))
            {
                if (Environment.UserInteractive)
                {
                    Logger.Debug("Returning username [DebugUser]", bus.CurrentMessage.UserName);
                    return "DebugUser";
                }

                Logger.Debug("Returning username [{0}\\{1}]", Environment.UserDomainName, Environment.UserName);
                return String.Format(@"{0}\{1}", Environment.UserDomainName, Environment.UserName);
            }

            Logger.Debug("Returning current message user name [{0}]", bus.CurrentMessage.UserName);
            return bus.CurrentMessage.UserName;
        }

        private static bool IsEvent(Type type)
        {
            if (type == null || type.Namespace == null)
                return false;

            return typeof (IDomainEvent).IsAssignableFrom(type);
        }

        private static bool IsCommand(Type type)
        {
            if (type == null || type.Namespace == null)
                return false;

            return typeof(ICommand).IsAssignableFrom(type);
        }
    }
}