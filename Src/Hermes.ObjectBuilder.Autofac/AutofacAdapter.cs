using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Builder;
using Hermes.Failover;
using Hermes.Ioc;
using Hermes.Logging;
using Microsoft.Practices.ServiceLocation;

using IContainer = Hermes.Ioc.IContainer;

namespace Hermes.ObjectBuilder.Autofac
{
    public class AutofacAdapter : ServiceLocatorImplBase, IContainerBuilder, IContainer
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(AutofacAdapter));

        protected readonly ILifetimeScope LifetimeScope;
        private bool disposed;

        public AutofacAdapter()
            :this(null)
        {
        }

        public AutofacAdapter(ILifetimeScope container)
        {
            if (container == null)
            {
                LifetimeScope = new ContainerBuilder().Build();
            }
            else
            {
                LifetimeScope = container;
                Logger.Debug("Starting new lifetime scope {0}", LifetimeScope.GetHashCode());
            }
        }

        ~AutofacAdapter()
        {
            Dispose(false);
        }

        public IContainer BuildContainer()
        {
            return this;
        }

        public virtual IContainer BeginLifetimeScope()
        {
            return new AutofacAdapter(LifetimeScope.BeginLifetimeScope());
        }

        public void RegisterModule(IRegisterDependencies module)
        {
            module.Register(this);
        }        

        public void RegisterSingleton(object instance) 
        {
            if (IsComponentAlreadyRegistered(instance.GetType()))
            {
                return;
            }

            var services = GetAllServices(instance.GetType());
            var builder = new ContainerBuilder();

            builder.RegisterInstance(instance).As(services)
                .PropertiesAutowired();

            builder.Update(LifetimeScope.ComponentRegistry);
        }        

        public void RegisterType<T>()
        {
            RegisterType(typeof(T));
        }

        public void RegisterType(Type type)
        {
            RegisterType(type, DependencyLifecycle.InstancePerUnitOfWork);
        }

        public void RegisterType(Type type, DependencyLifecycle dependencyLifecycle)
        {
            if (IsComponentAlreadyRegistered(type))
            {
                return;
            }

            var services = GetAllServices(type);

            var builder = new ContainerBuilder();
            var registration = builder.RegisterType(type)
                                      .As(services)
                                      .PropertiesAutowired();

            ConfigureLifetimeScope(dependencyLifecycle, registration);
            builder.Update(LifetimeScope.ComponentRegistry);
        }

        public void RegisterType<T>(DependencyLifecycle dependencyLifecycle)
        {
            ((IContainerBuilder)this).RegisterType(typeof (T), dependencyLifecycle);
        }

        protected virtual void ConfigureLifetimeScope<T>(DependencyLifecycle dependencyLifecycle, IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration)
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
            return LifetimeScope.ComponentRegistry.Registrations.FirstOrDefault(x => x.Activator.LimitType == concreteComponent) != null;
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            object instance = key != null
                ? LifetimeScope.ResolveNamed(key, serviceType)
                : LifetimeScope.Resolve(serviceType);

            return instance;
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            object instance = LifetimeScope.Resolve(enumerableType);

            return ((IEnumerable)instance).Cast<object>().ToArray();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing && LifetimeScope != null)
            {
                Logger.Debug("Disposing lifetime scope {0}", GetHashCode());
                LifetimeScope.Dispose();
            }

            disposed = true;
        }   
   
        public override int GetHashCode()
        {
            return LifetimeScope.GetHashCode();
        }
    }
}
