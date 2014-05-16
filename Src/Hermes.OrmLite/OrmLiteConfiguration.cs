using System;
using System.Configuration;
using Hermes.Ioc;
using Hermes.Messaging;
using ServiceStack.OrmLite;

namespace Hermes.OrmLite
{
    [Obsolete("No longer supported", true)]
    public static class OrmLiteConfiguration
    {
        public static IConfigureEndpoint UseOrmLite(this IConfigureEndpoint config, string connectionStringName)
        {
            Mandate.ParameterNotNullOrEmpty(connectionStringName, "connectionStringName", "Please provide a valid connection string name.");

            config.RegisterDependencies(new OrmLiteConfigurationRegistrar(connectionStringName));
            return config;
        }
    }

    [Obsolete("No longer supported", true)]
    public sealed class OrmLiteConfigurationRegistrar : IRegisterDependencies
    {
        private readonly string connectionStringName;

        public OrmLiteConfigurationRegistrar(string connectionStringName)
        {
            this.connectionStringName = connectionStringName;
        }

        public void Register(IContainerBuilder containerBuilder)
        {
            var connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            containerBuilder.RegisterSingleton(new OrmLiteConnectionFactory(connectionString, SqlServerDialect.Provider));
            containerBuilder.RegisterType<OrmLiteUnitOfWork>(DependencyLifecycle.InstancePerUnitOfWork);
        }
    }
}
