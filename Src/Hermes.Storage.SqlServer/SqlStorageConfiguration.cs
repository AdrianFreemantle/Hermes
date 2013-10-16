using Hermes.Ioc;
using Hermes.Messaging.Configuration;

namespace Hermes.Storage.SqlServer
{
    public static class SqlStorageConfiguration
    {
        public const string StorageConnectionStringKey = "Hermes.Storage.SqlServer.ConnectionString";

        public static IConfigureEndpoint UseSqlStorage(this IConfigureEndpoint config, string connectionString)
        {
            Settings.AddSetting(StorageConnectionStringKey, connectionString);
            config.RegisterDependancies(new SqlStorageDependancyRegistrar());
            return config;
        }

        private class SqlStorageDependancyRegistrar : IRegisterDependancies
        {
            public void Register(IContainerBuilder containerBuilder)
            {
                containerBuilder.RegisterType<SqlSubscriptionStorage>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<SqlTimeoutStorage>(DependencyLifecycle.SingleInstance);
            }
        }
    }
}
