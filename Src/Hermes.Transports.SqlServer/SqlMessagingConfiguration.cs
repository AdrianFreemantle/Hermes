using Hermes.Ioc;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;

namespace Hermes.Transports.SqlServer
{
    public static class SqlMessagingConfiguration 
    {
        public const string MessagingConnectionStringKey = "Hermes.Transports.SqlServer.ConnectionString";

        public static IConfigureEndpoint UseSqlTransport(this IConfigureEndpoint config, string connectionString)
        {
            if (!Settings.IsClientEndpoint)
            {
                Address.IgnoreMachineName();
            }

            Settings.Builder.RegisterType<SqlMessageDequeueStrategy>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SqlMessageSender>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SqlQueueCreator>(DependencyLifecycle.SingleInstance);

            Settings.AddSetting(MessagingConnectionStringKey, connectionString);
            return config;
        }
    }
}
