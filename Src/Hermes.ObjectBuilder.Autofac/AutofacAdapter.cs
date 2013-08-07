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
    public class AutofacAdapter : ServiceLocatorImplBase, IContainerBuilder, Ioc.IContainer
    {
        private readonly ILifetimeScope lifetimeScope;
        private bool disposed;

        public Ioc.IContainer Container { get { return this; } }

        public AutofacAdapter()
            :this(null)
        {
        }

        public AutofacAdapter(ILifetimeScope container)
        {
            if (container == null)
            {
                lifetimeScope = new ContainerBuilder().Build();
                ((IContainerBuilder)this).RegisterType<AutofacAdapter>(DependencyLifecycle.InstancePerLifetimeScope);
            }
            else
            {
                lifetimeScope = container;
            }
        }

        ~AutofacAdapter()
        {
            Dispose(false);
        }

        public Ioc.IContainer BeginLifetimeScope()
        {
            return new AutofacAdapter(lifetimeScope.BeginLifetimeScope());
        }

        void IContainerBuilder.RegisterHandlers(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
            {
                return;
            }

            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(assemblies.ToArray())
                   .AsClosedTypesOf(typeof(IHandleMessage<>)).InstancePerLifetimeScope();

            builder.Update(lifetimeScope.ComponentRegistry);
        }

        void IContainerBuilder.RegisterSingleton<T>(object instance)
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(instance).As<T>().PropertiesAutowired();
            builder.Update(lifetimeScope.ComponentRegistry);
        }

        void IContainerBuilder.RegisterType<T>(DependencyLifecycle dependencyLifecycle)
        {
            if (IsComponentAlreadyRegistered(typeof (T)))
            {
                return;
            }

            var services = GetAllServices(typeof(T));
            
            var builder = new ContainerBuilder();
            var registration = builder.RegisterType<T>().As(services).PropertiesAutowired();
            
            ConfigureLifetimeScope(dependencyLifecycle, registration);
            builder.Update(lifetimeScope.ComponentRegistry);
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
            return lifetimeScope.ComponentRegistry.Registrations.FirstOrDefault(x => x.Activator.LimitType == concreteComponent) != null;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            return key != null
                ? lifetimeScope.ResolveNamed(key, serviceType)
                : lifetimeScope.Resolve(serviceType);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            object instance = lifetimeScope.Resolve(enumerableType);

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

            if (disposing && lifetimeScope != null)
            {
                lifetimeScope.Dispose();
            }

            disposed = true;
        }      
    }
}
