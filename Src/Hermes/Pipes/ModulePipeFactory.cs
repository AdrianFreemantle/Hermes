using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Pipes
{
    public class ModulePipeFactory<T>
    {
        protected readonly List<Type> ModuleChain = new List<Type>();

        public virtual ModulePipeFactory<T> Add<TProcessor>() where TProcessor : IModule<T>
        {
            ModuleChain.Add(typeof(TProcessor));
            return this;
        }

        public virtual ModulePipe<T> Build(IServiceLocator serviceLocator)
        {
            var chain = new Queue<Type>();

            foreach (var type in ModuleChain)
            {
                chain.Enqueue(type);
            }

            return new ModulePipe<T>(chain, serviceLocator);
        }

        public virtual ModulePipe<T> Build()
        {
            return Build(ServiceLocator.Current);
        }
    }
}