using System.Data.Entity;

using Hermes.EntityFramework.Queries;
using Hermes.Ioc;
using Hermes.Messaging.Configuration;

namespace Hermes.EntityFramework
{
    public static class EntityFrameworkConfiguration
    {
        public static IConfigureEndpoint ConfigureEntityFramework<TContext>(this IConfigureEndpoint config, string connectionStringName)
            where TContext : DbContext, new()
        {
            Settings.Builder.RegisterSingleton<IContextFactory>(new ContextFactory<TContext>(connectionStringName));

            Settings.Builder.RegisterType<EntityFrameworkUnitOfWork>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<UnitOfWorkManager>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<DatabaseQuery>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<LookupTableFactory>(DependencyLifecycle.InstancePerLifetimeScope);
            Settings.Builder.RegisterType<DatabaseQuery>(DependencyLifecycle.InstancePerLifetimeScope);

            return config;
        }
    }
}
