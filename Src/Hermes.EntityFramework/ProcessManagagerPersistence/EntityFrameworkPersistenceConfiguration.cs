using System.Data.Entity;

using Hermes.Ioc;
using Hermes.Messaging.Configuration;

namespace Hermes.EntityFramework.ProcessManagagerPersistence
{
    public static class EntityFrameworkPersistenceConfiguration
    {
        public static IConfigureEndpoint UseEntityFrameworkSagaPersister<TContext>(this IConfigureEndpoint config, string connectionStringName)
            where TContext : DbContext, new()
        {
            Settings.Builder.RegisterSingleton<IContextFactory>(new ContextFactory<TContext>(connectionStringName));
            Settings.Builder.RegisterType<EntityFrameworkUnitOfWork>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<UnitOfWorkManager>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<ProcessManagerPersister>(DependencyLifecycle.InstancePerLifetimeScope);

            return config;
        }
    }
}