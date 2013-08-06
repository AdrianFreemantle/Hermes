using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac;
using Autofac.Builder;
using Hermes.Ioc;
using Hermes.Messages;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.ObjectBuilder.Autofac
{
    public class AutofacServiceAdapter : ServiceLocatorImplBase, IObjectBuilder
    {
        public ILifetimeScope container { get; set; }
        private bool disposed;

        public AutofacServiceAdapter()
            :this(null)
        {
        }

        public AutofacServiceAdapter(ILifetimeScope container)
        {
            this.container = this.container = container ?? new ContainerBuilder().Build();            
        }

        ~AutofacServiceAdapter()
        {
            Dispose(false);
        }

        void IObjectBuilder.RegisterHandlers(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
            {
                return;
            }

            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(assemblies.ToArray())
                   .AsClosedTypesOf(typeof(IHandleMessage<>)).InstancePerLifetimeScope();

            builder.Update(container.ComponentRegistry);
        }

        void IObjectBuilder.RegisterSingleton<T>(object instance)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance).As<T>().PropertiesAutowired();
            builder.Update(container.ComponentRegistry);
        }

        void IObjectBuilder.RegisterType<T>(DependencyLifecycle dependencyLifecycle)
        {
            if (IsComponentAlreadyRegistered(typeof (T)))
            {
                return;
            }

            var services = GetAllServices(typeof(T));
            
            var builder = new ContainerBuilder();
            var registration = builder.RegisterType<T>().As(services).PropertiesAutowired();
            
            ConfigureLifetimeScope(dependencyLifecycle, registration);
            builder.Update(container.ComponentRegistry);
        }

        private static void ConfigureLifetimeScope<T>(DependencyLifecycle dependencyLifecycle, IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration)
        {
            switch (dependencyLifecycle)
            {
                case DependencyLifecycle.SingleInstance:
                    registration.SingleInstance();
                    break;
                case DependencyLifecycle.InstancePerDependency:
                    registration.InstancePerDependency();
                    break;
                case DependencyLifecycle.InstancePerLifetimeScope:
                    registration.InstancePerLifetimeScope();
                    break;
                default:
                    throw new ArgumentException("Unknown container lifecycle - " + dependencyLifecycle);
            }
        }

        static Type[] GetAllServices(Type type)
        {
            if (type == null)
            {
                return new Type[0];
            }

            var result = new List<Type>(type.GetInterfaces()) {
                type
            };

            foreach (Type interfaceType in type.GetInterfaces())
            {
                result.AddRange(GetAllServices(interfaceType));
            }

            return result.Distinct().ToArray();
        }

        private bool IsComponentAlreadyRegistered(Type concreteComponent)
        {
            return container.ComponentRegistry.Registrations.FirstOrDefault(x => x.Activator.LimitType == concreteComponent) != null;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            return key != null
                ? container.ResolveNamed(key, serviceType)
                : container.Resolve(serviceType);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            object instance = container.Resolve(enumerableType);

            return ((IEnumerable)instance).Cast<object>();
        }        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing && container != null)
            {
                container.Dispose();
            }

            disposed = true;
        }

        public IObjectBuilder BeginLifetimeScope()
        {
            return new AutofacServiceAdapter(container.BeginLifetimeScope());
        }
    }
}
