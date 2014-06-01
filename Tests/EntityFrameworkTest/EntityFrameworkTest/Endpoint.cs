using System;
using EntityFrameworkTest.Model;
using EntityFrameworkTest.Queries.ComanyDtoQueries;
using EntityFrameworkTest.Queries.DyanamicCompanyQueries;
using Hermes.EntityFramework;
using Hermes.Ioc;
using Hermes.Messaging;
using Hermes.Messaging.EndPoints;
using Hermes.Messaging.Transports.SqlTransport;
using Hermes.ObjectBuilder.Autofac;
using Hermes.Serialization.Json;

namespace EntityFrameworkTest
{
    public class Endpoint : LocalEndpoint<AutofacAdapter>
    {
        protected override void ConfigureEndpoint(IConfigureEndpoint configuration)
        {
            configuration
                .UseJsonSerialization()
                .RegisterDependencies(new QueryServiceRegistrar())
                .UserNameResolver(GetCurrentUserName)
                .UseSqlTransport("SqlTransport")
                .ConfigureEntityFramework<EntityFrameworkTestContext>("EntityFrameworkTest");
        }

        private static string GetCurrentUserName()
        {
            return Environment.UserName;
        }
    }

    public class QueryServiceRegistrar : IRegisterDependencies 
    {
        public void Register(IContainerBuilder containerBuilder)
        {
            //these two services are not automatically registered as they do not implement IAnswerQuery
            containerBuilder.RegisterType<DtoCompanyQueryService>(DependencyLifecycle.InstancePerUnitOfWork);
            containerBuilder.RegisterType<DynamicCompanyQueryService>(DependencyLifecycle.InstancePerUnitOfWork);
        }
    }
}