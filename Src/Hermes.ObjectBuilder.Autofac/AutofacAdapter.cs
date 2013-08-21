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

namespace Hermes.ObjectBuilder.Autofac
{
    public class AutofacAdapter : ServiceLocatorImplBase, IContainerBuilder, Ioc.IContainer
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(AutofacAdapter));

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

            Logger.Debug("Starting new container {0}", lifetimeScope.GetHashCode());
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

            LogServiceType(serviceType);

            return key != null
                ? lifetimeScope.ResolveNamed(key, serviceType)
                : lifetimeScope.Resolve(serviceType);
        }

        private static string GetGenericParametersString(Type serviceType)
        {
             var genericArguments = serviceType.GenericTypeArguments.Select(type => type.Name.ToString(CultureInfo.InvariantCulture));
             return String.Join(", ", genericArguments);
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException("serviceType");
            }

            LogServiceType(serviceType);

            var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
            object instance = lifetimeScope.Resolve(enumerableType);

            return ((IEnumerable)instance).Cast<object>();
        }

        private void LogServiceType(Type serviceType)
        {
            string genericParametrs = GetGenericParametersString(serviceType);

            if (genericParametrs.Length > 0)
            {
                Logger.Debug("Resolving service {0}<{1}> from container {2}", serviceType.Name, genericParametrs, GetHashCode());
            }
            else
            {
                Logger.Debug("Resolving service {0} from container {1}", serviceType.Name, GetHashCode());
            }
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
                Logger.Debug("Disposing container {0}", GetHashCode());
                lifetimeScope.Dispose();
            }

            disposed = true;
        }   
   
        public override int GetHashCode()
        {
            return lifetimeScope.GetHashCode();
        }
    }
}
