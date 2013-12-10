using System;
using System.Collections.Generic;
using Microsoft.Practices.ServiceLocation;

namespace Hermes.Pipes
{
    public class ModuleStack<T>
    {
        protected readonly List<Type> ModuleChain = new List<Type>();

        public virtual ModuleStack<T> Add<TProcessor>() where TProcessor : IModule<T>
        {
            ModuleChain.Add(typeof(TProcessor));
            return this;
        }

        public virtual ModuleChain<T> ToModuleChain(IServiceLocator serviceLocator)
        {
            var chain = new Queue<Type>();

            foreach (var type in ModuleChain)
            {
                chain.Enqueue(type);
            }

            return new ModuleChain<T>(chain, serviceLocator);
        }
    }
}