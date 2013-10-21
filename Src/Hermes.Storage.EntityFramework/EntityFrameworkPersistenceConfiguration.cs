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
            config.RegisterDependencies(new ProcessManagagerStorageDependencyRegistrar());

            return config;
        }

        public static IConfigureEndpoint UseEntityFrameworkKeyValueStorage(this IConfigureEndpoint config)
        {
            config.RegisterDependencies(new KeyValueStorageDependencyRegistrar());

            return config;
        }

        private class ProcessManagagerStorageDependencyRegistrar : IRegisterDependencies
        {
            public void Register(IContainerBuilder containerBuilder)
            {
                containerBuilder.RegisterType<KeyValueStorePersister>(DependencyLifecycle.InstancePerUnitOfWork);
            }
        }

        private class KeyValueStorageDependencyRegistrar : IRegisterDependencies
        {
            public void Register(IContainerBuilder containerBuilder)
            {
                containerBuilder.RegisterType<ProcessManagerPersister>(DependencyLifecycle.InstancePerUnitOfWork);
            }
        }
    }
}