using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using Autofac;
using Autofac.Builder;

using Hermes.Ioc;
using Hermes.Logging;
using Hermes.Messaging;

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

        public IContainer BeginLifetimeScope()
        {
            return new AutofacAdapter(LifetimeScope.BeginLifetimeScope());
        }

        public void RegisterMessageHandlers(IEnumerable<Assembly> assemblies)
        {
            if (assemblies == null)
            {
                return;
            }

            var builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(assemblies.ToArray())
                   .AsClosedTypesOf(typeof (IHandleMessage<>))
                   .InstancePerLifetimeScope()
                   .PropertiesAutowired()
                   .AsImplementedInterfaces();  
            
            builder.Update(LifetimeScope.ComponentRegistry);
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

            LogServiceType(instance);

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

            var services = ((IEnumerable)instance).Cast<object>().ToList();

            foreach (var service in services)
            {
                LogServiceType(service);
            }

            return ((IEnumerable)instance).Cast<object>();
        }

        private void LogServiceType(object service)
        {
            var serviceType = service.GetType();

            string genericParametrs = GetGenericParametersString(serviceType);

            if (genericParametrs.Length > 0)
            {
                Logger.Debug("Activated service {0}<{1}> : {2} from lifetime scope {3}", serviceType.Name, genericParametrs, service.GetHashCode(), GetHashCode());
            }
            else
            {
                Logger.Info("Activated service {0} {1} from lifetime scope {2}", serviceType.Name, service.GetHashCode(), GetHashCode());
            }
        }

        private static string GetGenericParametersString(Type serviceType)
        {
            var genericArguments = serviceType.GenericTypeArguments.Select(type => type.Name.ToString(CultureInfo.InvariantCulture));
            return String.Join(", ", genericArguments);
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
