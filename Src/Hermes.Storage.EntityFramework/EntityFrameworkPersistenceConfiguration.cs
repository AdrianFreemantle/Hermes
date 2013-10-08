using Hermes.Ioc;
using Hermes.Messaging.Configuration;
using Hermes.Storage.EntityFramework.KeyValueStore;
using Hermes.Storage.EntityFramework.ProcessManagager;

namespace Hermes.Storage.EntityFramework
{   
    public static class EntityFrameworkPersistenceConfiguration
    {
        public static IConfigureEndpoint UseEntityFrameworkProcessManagagerStorage(this IConfigureEndpoint config)
        {
            Settings.Builder.RegisterType<ProcessManagerPersister>(DependencyLifecycle.InstancePerLifetimeScope);

            return config;
        }

        public static IConfigureEndpoint UseEntityFrameworkKeyValueStorage(this IConfigureEndpoint config)
        {
            Settings.Builder.RegisterType<KeyValueStorePersister>(DependencyLifecycle.InstancePerLifetimeScope);

            return config;
        }
    }
}