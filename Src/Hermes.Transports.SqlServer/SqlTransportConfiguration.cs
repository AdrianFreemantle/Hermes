using System.Configuration;
using Hermes.Ioc;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;

namespace Hermes.Transports.SqlServer
{
    public static class SqlTransportConfiguration 
    {
        public const string MessagingConnectionStringKey = "Hermes.Transports.SqlServer.ConnectionString";

        public static IConfigureEndpoint UseSqlTransport(this IConfigureEndpoint config)
        {
            return UseSqlTransport(config, "SqlTransport");
        }

        public static IConfigureEndpoint UseSqlTransport(this IConfigureEndpoint config, string connectionStringName)
        {
            Address.IgnoreMachineName();

            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            Settings.AddSetting(MessagingConnectionStringKey, connectionString);

            if (Settings.IsClientEndpoint)
            {
                Address.InitializeLocalAddress(Address.Local.Queue + "." + Address.Local.Machine);
            }

            config.RegisterDependencies(new SqlMessagingDependencyRegistrar());

            return config;
        }

        private class SqlMessagingDependencyRegistrar : IRegisterDependencies
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
