using Hermes.Configuration;
using Hermes.Ioc;

namespace Hermes.Transports.SqlServer
{
    public static class SqlMessagingConfiguration 
    {
        public const string MessagingConnectionStringKey = "Hermes.Transports.SqlServer.ConnectionString";

        public static IConfigureBus UsingSqlTransport(this IConfigureBus config, string connectionString)
        {
            Settings.Builder.RegisterType<SqlMessageDequeueStrategy>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SqlMessageSender>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SqlMessageReceiver>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SqlSubscriptionStorage>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SqlMessagePublisher>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SqlQueueCreator>(DependencyLifecycle.SingleInstance);

            Settings.AddSetting(MessagingConnectionStringKey, connectionString);
            return config;
        }
    }
}
