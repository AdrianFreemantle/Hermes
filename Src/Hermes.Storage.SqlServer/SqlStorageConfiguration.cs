using Hermes.Configuration;
using Hermes.Ioc;
using Hermes.Messaging;

namespace Hermes.Storage.SqlServer
{
    public static class SqlStorageConfiguration
    {
        public const string StorageConnectionStringKey = "Hermes.Storage.SqlServer.ConnectionString";

        public static IConfigureEndpoint UseSqlStorage(this IConfigureEndpoint config, string connectionString)
        {
            Address.IgnoreMachineName();

            Settings.Builder.RegisterType<SqlSubscriptionStorage>(DependencyLifecycle.SingleInstance);
            Settings.Builder.RegisterType<SqlTimeoutStorage>(DependencyLifecycle.SingleInstance);
            Settings.AddSetting(StorageConnectionStringKey, connectionString);
            return config;
        }
    }
}
