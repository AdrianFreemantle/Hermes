using System.Configuration;
using Hermes.Ioc;
using Hermes.Messaging.Configuration;

namespace Hermes.Storage.SqlServer
{
    public static class SqlStorageConfiguration
    {
        public const string StorageConnectionStringKey = "Hermes.Storage.SqlServer.ConnectionString";

        public static IConfigureEndpoint UseSqlStorage(this IConfigureEndpoint config)
        {
            return UseSqlStorage(config, "SqlStorage");
        }

        public static IConfigureEndpoint UseSqlStorage(this IConfigureEndpoint config, string connectionStringName)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            
            Settings.AddSetting(StorageConnectionStringKey, connectionString);
            config.RegisterDependencies(new SqlStorageDependencyRegistrar());
            return config;
        }        

        private class SqlStorageDependencyRegistrar : IRegisterDependencies
        {
            public void Register(IContainerBuilder containerBuilder)
            {
                containerBuilder.RegisterType<SqlSubscriptionStorage>(DependencyLifecycle.SingleInstance);
                containerBuilder.RegisterType<SqlTimeoutStorage>(DependencyLifecycle.SingleInstance);
            }
        }
    }
}
