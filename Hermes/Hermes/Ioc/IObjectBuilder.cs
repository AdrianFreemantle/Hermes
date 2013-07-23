using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Practices.ServiceLocation;

namespace Hermes.Ioc
{
    public interface IObjectBuilder : IServiceLocator, IDisposable
    {
        IObjectBuilder BeginLifetimeScope();
        void RegisterType<T>(DependencyLifecycle dependencyLifecycle);
        void RegisterSingleton<T>(object instance);
        void RegisterHandlers(IEnumerable<Assembly> assemblies);
    }
}
