using System.Data.Entity;

using Hermes.EntityFramework.KeyValueStore;
using Hermes.EntityFramework.ProcessManagager;
using Hermes.EntityFramework.Queries;
using Hermes.EntityFramework.Queues;
using Hermes.Ioc;
using Hermes.Messaging;

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
            ContextFactory<TContext>.ConnectionStringName = connectionStringName;

            containerBuilder.RegisterType<ContextFactory<TContext>>();
            containerBuilder.RegisterType<EntityFrameworkUnitOfWork>( );
            containerBuilder.RegisterType<DatabaseQuery>( );
            containerBuilder.RegisterType<KeyValueStorePersister>( );
            containerBuilder.RegisterType<ProcessManagerPersister>( );
            containerBuilder.RegisterType<AggregateRepository>( );
            containerBuilder.RegisterType<QueueFactory>( );
            containerBuilder.RegisterType<QueueStore>( );
        }
    }
}
