using System.Data.Entity;

using Hermes.EntityFramework.KeyValueStore;
using Hermes.EntityFramework.ProcessManagager;
using Hermes.EntityFramework.Queries;
using Hermes.Ioc;
using Hermes.Messaging;
using Hermes.Messaging.Configuration;

namespace Hermes.EntityFramework
{
    public static class EntityFrameworkConfiguration
    {
        public static IConfigureEndpoint ConfigureEntityFramework<TContext>(this IConfigureEndpoint config, string connectionStringName = null)
            where TContext : DbContext, new()
        {
            config.RegisterDependencies(new EntityFrameworkConfigurationRegistrar<TContext>(connectionStringName));
            return config;
        }        
    }

    public sealed class EntityFrameworkConfigurationRegistrar<TContext>
            : IRegisterDependencies where TContext : DbContext, new()
    {
        private readonly string connectionStringName;

        public EntityFrameworkConfigurationRegistrar(string connectionStringName)
        {
            this.connectionStringName = connectionStringName;
        }

        public void Register(IContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterSingleton(new ContextFactory<TContext>(connectionStringName));

            containerBuilder.RegisterType<EntityFrameworkUnitOfWork>(DependencyLifecycle.InstancePerUnitOfWork);
            containerBuilder.RegisterType<DatabaseQuery>(DependencyLifecycle.InstancePerUnitOfWork);
            containerBuilder.RegisterType<KeyValueStorePersister>(DependencyLifecycle.InstancePerUnitOfWork);
            containerBuilder.RegisterType<ProcessManagerPersister>(DependencyLifecycle.InstancePerUnitOfWork);
            containerBuilder.RegisterType<AggregateRepository>(DependencyLifecycle.InstancePerUnitOfWork);
        }
    }
}
