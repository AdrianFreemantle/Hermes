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
            Address.IgnoreMachineName();
            Settings.AddSetting(MessagingConnectionStringKey, connectionString);

            if (Settings.IsClientEndpoint)
            {
                Address.InitializeLocalAddress(Address.Local.Queue + "." + Address.Local.Machine);
            }

            config.RegisterDependancies(new SqlMessagingDependancyRegistrar());

            return config;
        }

        private class SqlMessagingDependancyRegistrar : IRegisterDependancies
        {
            public void Register(IContainerBuilder containerBuilder)
            {
                containerBuilder.RegisterType<SqlMessageDequeStrategy>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<SqlMessageSender>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<SqlQueueCreator>(DependencyLifecycle.SingleInstance);
            }
        }
    }
}
