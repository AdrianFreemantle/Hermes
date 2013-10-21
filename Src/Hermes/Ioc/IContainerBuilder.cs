using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hermes.Ioc
{
    public interface IContainerBuilder : IDisposable
    {
        IContainer BuildContainer();
        void RegisterType<T>(DependencyLifecycle dependencyLifecycle);
        void RegisterType(Type type, DependencyLifecycle dependencyLifecycle);
        void RegisterSingleton(object instance);
        void RegisterMessageHandlers(IEnumerable<Assembly> assemblies);
        void RegisterModule(IRegisterDependencies module);
    }
}
