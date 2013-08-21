using Hermes.Configuration;
using Hermes.Ioc;

namespace Hermes.Storage.SqlServer
{
    public static class SqlStorageConfiguration
    {
        public const string StorageConnectionStringKey = "Hermes.Storage.SqlServer.ConnectionString";

        public static IConfigureBus UsingSqlStorage(this IConfigureBus config, string connectionString)
        {
            Settings.Builder.RegisterType<SqlSubscriptionStorage>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SqlTimeoutStorage>(DependencyLifecycle.SingleInstance);

            Settings.AddSetting(StorageConnectionStringKey, connectionString);
            return config;
        }
    }
}
