using System;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Integration.Mvc;
using Hermes.Ioc;
using Hermes.ObjectBuilder.Autofac;

namespace Starbucks
{
    public class MvcAutofacAdapter : AutofacAdapter
    {
        public MvcAutofacAdapter()
            : base(ConfigureMvcDependencies())
        {
        }

        private static Autofac.IContainer ConfigureMvcDependencies()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.RegisterModelBinders(Assembly.GetExecutingAssembly());
            builder.RegisterModelBinderProvider();

            builder.RegisterFilterProvider();
            return builder.Build();
        }

        public AutofacDependencyResolver BuildAutofacDependencyResolver()
        {
            return new AutofacDependencyResolver(LifetimeScope);
        }

        protected override void ConfigureLifetimeScope<T>(DependencyLifecycle dependencyLifecycle, IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration)
        {
            switch (dependencyLifecycle)
            {
                case DependencyLifecycle.SingleInstance:
                    registration.SingleInstance();
                    break;
                case DependencyLifecycle.InstancePerDependency:
                    registration.InstancePerDependency();
                    break;
                case DependencyLifecycle.InstancePerUnitOfWork:
                    registration.InstancePerHttpRequest();
                    break;
                default:
                    throw new ArgumentException("Unknown container lifecycle - " + dependencyLifecycle);
            }
        }
    }
}