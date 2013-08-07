using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hermes.Ioc
{
    public interface IContainerBuilder : IDisposable
    {
        IContainer Container { get; }
        void RegisterType<T>(DependencyLifecycle dependencyLifecycle);
        void RegisterSingleton<T>(object instance);
        void RegisterHandlers(IEnumerable<Assembly> assemblies);
    }
}
