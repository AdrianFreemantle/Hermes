using Hermes.Ioc;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;

namespace Hermes.Storage.SqlServer
{
    public static class SqlStorageConfiguration
    {
        public const string StorageConnectionStringKey = "Hermes.Storage.SqlServer.ConnectionString";

        public static IConfigureEndpoint UseSqlStorage(this IConfigureEndpoint config, string connectionString)
        {
            Settings.Builder.RegisterType<SqlSubscriptionStorage>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SqlTimeoutStorage>(DependencyLifecycle.SingleInstance);
            Settings.AddSetting(StorageConnectionStringKey, connectionString);
            return config;
        }
    }
}
