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
            config.RegisterDependancies(new ProcessManagagerStorageDependancyRegistrar());

            return config;
        }

        public static IConfigureEndpoint UseEntityFrameworkKeyValueStorage(this IConfigureEndpoint config)
        {
            config.RegisterDependancies(new KeyValueStorageDependancyRegistrar());

            return config;
        }

        private class ProcessManagagerStorageDependancyRegistrar : IRegisterDependancies
        {
            public void Register(IContainerBuilder containerBuilder)
            {
                containerBuilder.RegisterType<KeyValueStorePersister>(DependencyLifecycle.InstancePerUnitOfWork);
            }
        }

        private class KeyValueStorageDependancyRegistrar : IRegisterDependancies
        {
            public void Register(IContainerBuilder containerBuilder)
            {
                containerBuilder.RegisterType<ProcessManagerPersister>(DependencyLifecycle.InstancePerUnitOfWork);
            }
        }
    }
}