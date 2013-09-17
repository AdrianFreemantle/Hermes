using System.Reflection;
using System.Web.Mvc;

using Autofac;
using Autofac.Integration.Mvc;

using Clientele.Core.Persistance;
using Clientele.DocumentTracking.DataModel;
using Clientele.Infrastructure;

namespace DocumentTracking.Presentation.Mvc.Startup
{
    public static class Wireup
    {
        static Wireup()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<EntityFrameworkUnitOfWork>().As<IUnitOfWork>().InstancePerHttpRequest();
            
            containerBuilder.RegisterType<ContextFactory<DocumentTrackingContext>>().As<IContextFactory>()
                            .As<IContextFactory>()
                            .WithParameter("connectionStringName", "DocumentTracking")
                            .PropertiesAutowired()
                            .InstancePerHttpRequest();

            containerBuilder.RegisterFilterProvider();
            containerBuilder.RegisterControllers(Assembly.GetExecutingAssembly());
            IContainer container = containerBuilder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        public static void Configure()
        {
           //calling this triggers the static constructor and that means configuration will only happen once. 
        }
    }
}